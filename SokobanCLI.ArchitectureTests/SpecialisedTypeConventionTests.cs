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
    /// Enforces mapping, enumeration, serialisation, and logging conventions
    /// that are statically decidable for this repository.
    /// </summary>
    [TestFixture]
    public sealed class SpecialisedTypeConventionTests
    {
        private static readonly string[] MappingMethodPrefixes = [ "To" ];

        [Test]
        public void GivenTheMappingExtensions_WhenParsed_ThenTheyUseDedicatedFilesAndNamespaces()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (!type.Identifier.ValueText.EndsWith("MappingExtensions", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;
                    string namespaceName = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

                    if (!namespaceName.EndsWith(".Mapping", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(type, $"mapping class '{type.Identifier.ValueText}' must live in a '.Mapping' namespace."));
                    }

                    if (!string.Equals(System.IO.Path.GetFileName(tree.FilePath), type.Identifier.ValueText + ".cs", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(type, "mapping class must live in its own identically named file."));
                    }
                }
            }

            ProductionSources.AssertClean("Mapping class placement", matched, violations);
        }

        [Test]
        public void GivenTheMappingExtensions_WhenParsed_ThenTheyAreStaticAndContainOnlyExtensionMethods()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (!type.Identifier.ValueText.EndsWith("MappingExtensions", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    if (!type.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        violations.Add(ProductionSources.Describe(type, $"mapping class '{type.Identifier.ValueText}' must be static."));
                    }

                    foreach (MethodDeclarationSyntax method in type.Members.OfType<MethodDeclarationSyntax>())
                    {
                        ParameterSyntax? receiver = method.ParameterList.Parameters.FirstOrDefault();

                        if (receiver is null || !receiver.Modifiers.Any(SyntaxKind.ThisKeyword))
                        {
                            violations.Add(ProductionSources.Describe(method, $"mapping method '{method.Identifier.ValueText}' must be an extension method."));
                        }

                        if (!method.Identifier.ValueText.StartsWith('T'))
                        {
                            violations.Add(ProductionSources.Describe(method, $"mapping method '{method.Identifier.ValueText}' must start with 'To'."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("Mapping extension shape", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoMappingMethodsLiveOutsideMappingExtensions()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (type.Identifier.ValueText.EndsWith("MappingExtensions", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    foreach (MethodDeclarationSyntax method in type.Members.OfType<MethodDeclarationSyntax>())
                    {
                        matched += 1;

                        if (MappingMethodPrefixes.Any(prefix => method.Identifier.ValueText.StartsWith(prefix, StringComparison.Ordinal)) &&
                            method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(SyntaxKind.ThisKeyword) == true)
                        {
                            violations.Add(ProductionSources.Describe(method, $"extension mapping method '{method.Identifier.ValueText}' must live in a *MappingExtensions class."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("Mapping methods colocated with mappings", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEnumerationsLiveOutsideDedicatedEnumerationNamespaces()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (EnumDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    matched += 1;
                    string namespaceName = declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

                    if (namespaceName.EndsWith(".Enumerations", StringComparison.Ordinal) ||
                        namespaceName.EndsWith(".Enums", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(declaration, $"enum '{declaration.Identifier}' must be colocated with its domain model."));
                    }
                }
            }

            ProductionSources.AssertClean("Enumeration placement", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEnumValuesAreUnique()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (EnumDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    matched += 1;

                    List<long> explicitValues = [];

                    foreach (EnumMemberDeclarationSyntax member in declaration.Members)
                    {
                        if (member.EqualsValue?.Value is not LiteralExpressionSyntax literal ||
                            !literal.Token.ValueText.All(char.IsDigit))
                        {
                            continue;
                        }

                        long value = long.Parse(literal.Token.ValueText, System.Globalization.CultureInfo.InvariantCulture);

                        if (explicitValues.Contains(value))
                        {
                            violations.Add(ProductionSources.Describe(member, $"enum '{declaration.Identifier}' repeats the value {value}."));
                        }

                        explicitValues.Add(value);
                    }
                }
            }

            ProductionSources.AssertClean("Unique enum values", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenDateHandlingPassesACulture()
        {
            string[] cultureSensitiveMethods = [ "Parse", "TryParse", "ParseExact", "TryParseExact", "ToString" ];
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    {
                        continue;
                    }

                    string methodName = memberAccess.Name.Identifier.ValueText;

                    if (!cultureSensitiveMethods.Contains(methodName, StringComparer.Ordinal))
                    {
                        continue;
                    }

                    if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
                        !identifier.Identifier.ValueText.Contains("DateTime", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    bool passesCulture = invocation.ArgumentList.Arguments.Any(argument =>
                        argument.Expression.ToString().Contains("CultureInfo", StringComparison.Ordinal));

                    if (!passesCulture)
                    {
                        violations.Add(ProductionSources.Describe(invocation, $"'{methodName}' must receive an explicit culture."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoJsonSerializerOptionsAreAllocatedInLoops()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    if (!string.Equals(creation.Type.ToString(), "JsonSerializerOptions", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool insideLoop = creation.Ancestors().Any(ancestor =>
                        ancestor is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax);

                    if (insideLoop)
                    {
                        violations.Add(ProductionSources.Describe(creation, "JsonSerializerOptions allocated inside a loop."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenArgumentsAndLocalsAreNotNamedAfterSensitiveData()
        {
            string[] sensitiveFragments = [ "password", "secret", "token", "apiKey", "cardNumber" ];
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ParameterSyntax parameter in tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>())
                {
                    matched += 1;

                    if (sensitiveFragments.Any(fragment => parameter.Identifier.ValueText.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                    {
                        violations.Add(ProductionSources.Describe(parameter, $"parameter '{parameter.Identifier.ValueText}' names sensitive data; such values must not be logged."));
                    }
                }
            }

            ProductionSources.AssertClean("Sensitive naming", matched, violations);
        }
    }
}