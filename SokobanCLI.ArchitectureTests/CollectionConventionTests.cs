using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Enforces the collection conventions of <c>csharp.collections.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class CollectionConventionTests
    {
        private static readonly string[] ConcreteCollectionTypes =
        [
            "List", "IList", "IReadOnlyList", "HashSet", "Dictionary", "IReadOnlyCollection", "ICollection"
        ];

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenCollectionMembersExposeEnumerable()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (!TryGetCollectionTypeName(method.ReturnType, out string? concreteType))
                    {
                        continue;
                    }

                    matched += 1;

                    if (!string.Equals(concreteType, "IEnumerable", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(method, $"returns '{concreteType}<T>'; use IEnumerable<T>."));
                    }
                }

                foreach (ParameterSyntax parameter in root.DescendantNodes().OfType<ParameterSyntax>())
                {
                    if (parameter.Type is null || !TryGetCollectionTypeName(parameter.Type, out string? concreteType))
                    {
                        continue;
                    }

                    // Arrays are not one of the prohibited concrete collection interfaces.
                    if (string.Equals(concreteType, "Array", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    if (!string.Equals(concreteType, "IEnumerable", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(parameter, $"parameter '{parameter.Identifier.ValueText}' uses '{concreteType}<T>'; use IEnumerable<T>."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenXmlSerialisedCollectionsUseList()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (PropertyDeclarationSyntax property in tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    matched += 1;

                    if (property.Type is not GenericNameSyntax generic)
                    {
                        continue;
                    }

                    string typeName = generic.Identifier.ValueText;

                    if (typeName is "List" or "IEnumerable" or "IReadOnlyList")
                    {
                        continue;
                    }

                    if (!ConcreteCollectionTypes.Contains(typeName, StringComparer.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(property, $"serialised collection property '{property.Identifier.ValueText}' uses '{typeName}<T>'."));
                }
            }

            ProductionSources.AssertClean("XML-serialised collection typing", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenCollectionInitialisationUsesCollectionExpressions()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    string typeName = creation.Type.ToString();

                    if (!ConcreteCollectionTypes.Contains(typeName, StringComparer.Ordinal) &&
                        !typeName.EndsWith("[]", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (creation.ArgumentList is not null && creation.ArgumentList.Arguments.Count > 0)
                    {
                        continue;
                    }

                    matched += 1;
                    violations.Add(ProductionSources.Describe(creation, $"'{typeName}' must be initialised with a collection expression."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoRedundantCollectionLookupLoopsAreUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ForEachStatementSyntax loop in tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>())
                {
                    matched += 1;

                    if (loop.Expression is InvocationExpressionSyntax invocation &&
                        invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.ValueText is "Where" or "Select" or "OrderBy")
                    {
                        violations.Add(ProductionSources.Describe(loop, "foreach over a LINQ query; use the query directly."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        private static bool TryGetCollectionTypeName(TypeSyntax type, out string? typeName)
        {
            typeName = null;

            if (type is GenericNameSyntax generic)
            {
                typeName = generic.Identifier.ValueText;
                return true;
            }

            if (type is QualifiedNameSyntax qualified && qualified.Right is GenericNameSyntax qualifiedGeneric)
            {
                typeName = qualifiedGeneric.Identifier.ValueText;
                return true;
            }

            if (type is ArrayTypeSyntax)
            {
                typeName = "Array";
                return true;
            }

            return false;
        }
    }
}