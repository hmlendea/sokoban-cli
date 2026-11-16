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
    /// Enforces the interface, type-declaration, property, method, and analyser-prevention rules
    /// of <c>csharp.coding.instructions.md</c> and <c>csharp.interfaces.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class TypeDeclarationConventionTests
    {
        private static readonly string[] InheritanceFriendlyTypeNames =
        [
            "Screen", "MenuScreen", "UiElement", "UiMenuItem", "Exception"
        ];

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoTypeIsDeclaredPartial()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BaseTypeDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
                {
                    matched += 1;

                    if (type.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        violations.Add(ProductionSources.Describe(type, $"type '{type.Identifier}' is partial."));
                    }
                }
            }

            ProductionSources.AssertClean("No partial types", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoPropertyUsesAnInitAccessor()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (AccessorDeclarationSyntax accessor in tree.GetRoot().DescendantNodes().OfType<AccessorDeclarationSyntax>())
                {
                    matched += 1;

                    if (accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                    {
                        violations.Add(ProductionSources.Describe(accessor, "'init' accessor is prohibited; use 'set'."));
                    }
                }
            }

            ProductionSources.AssertClean("No init accessors", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEveryClassIsSealedOrInheritableByDesign()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (type.Modifiers.Any(SyntaxKind.StaticKeyword) || type.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        continue;
                    }

                    matched += 1;

                    if (type.Modifiers.Any(SyntaxKind.SealedKeyword))
                    {
                        continue;
                    }

                    bool isBaseType = InheritanceFriendlyTypeNames.Contains(type.Identifier.ValueText, StringComparer.Ordinal);
                    bool isEnumerationClass = type.Members
                        .OfType<ConstructorDeclarationSyntax>()
                        .Any(constructor => constructor.Modifiers.Any(SyntaxKind.PrivateKeyword));

                    if (!isBaseType && !isEnumerationClass)
                    {
                        violations.Add(ProductionSources.Describe(type, $"class '{type.Identifier.ValueText}' must be sealed."));
                    }
                }
            }

            ProductionSources.AssertClean("Sealed classes", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEveryNonStaticClassWithPublicMethodsHasAnInterface()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (type.Modifiers.Any(SyntaxKind.StaticKeyword) || type.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        continue;
                    }

                    string namespaceName = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

                    if (string.Equals(namespaceName, "SokobanCLI.Models", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool hasPublicMethods = type.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Any(method => method.Modifiers.Any(SyntaxKind.PublicKeyword));

                    if (!hasPublicMethods)
                    {
                        continue;
                    }

                    matched += 1;
                    string expectedInterface = "I" + type.Identifier.ValueText;

                    bool implementsInterface = type.BaseList is not null &&
                        type.BaseList.Types
                            .Select(baseType => baseType.Type.ToString())
                            .Any(name => string.Equals(name, expectedInterface, StringComparison.Ordinal));

                    if (!implementsInterface)
                    {
                        violations.Add(ProductionSources.Describe(type, $"class '{type.Identifier.ValueText}' must implement '{expectedInterface}'."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenImplementationsMatchInterfaceParameterNames()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (type.BaseList is null)
                    {
                        continue;
                    }

                    foreach (BaseTypeSyntax baseType in type.BaseList.Types)
                    {
                        string interfaceName = baseType.Type.ToString();

                        if (!interfaceName.StartsWith('I') || interfaceName.Length < 2 || !char.IsUpper(interfaceName[1]))
                        {
                            continue;
                        }

                        InterfaceDeclarationSyntax? contract = FindInterface(tree, interfaceName);

                        if (contract is null)
                        {
                            continue;
                        }

                        foreach (MemberDeclarationSyntax member in contract.Members)
                        {
                            matched += 1;

                            if (member is not MethodDeclarationSyntax interfaceMethod)
                            {
                                continue;
                            }

                            MethodDeclarationSyntax? implementation = type.Members
                                .OfType<MethodDeclarationSyntax>()
                                .FirstOrDefault(method => string.Equals(method.Identifier.ValueText, interfaceMethod.Identifier.ValueText, StringComparison.Ordinal));

                            if (implementation is null)
                            {
                                violations.Add(ProductionSources.Describe(type, $"class '{type.Identifier.ValueText}' does not implement '{interfaceName}.{interfaceMethod.Identifier.ValueText}'."));
                                continue;
                            }

                            List<string> interfaceParameters = interfaceMethod.ParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText).ToList();
                            List<string> implementationParameters = implementation.ParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText).ToList();

                            if (!interfaceParameters.SequenceEqual(implementationParameters, StringComparer.Ordinal))
                            {
                                violations.Add(ProductionSources.Describe(implementation, $"parameter names differ from '{interfaceName}.{interfaceMethod.Identifier.ValueText}'."));
                            }
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenDomainTypesWithEqualityImplementTheContract()
        {
            string[] requiredMembers = [ "Equals", "GetHashCode", "operator ==", "operator !=" ];
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (StructDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>())
                {
                    bool implementsEquatable = type.BaseList is not null &&
                        type.BaseList.Types.Any(baseType => baseType.Type.ToString().StartsWith("IEquatable", StringComparison.Ordinal));

                    if (!implementsEquatable)
                    {
                        continue;
                    }

                    matched += 1;

                    foreach (string requiredMember in requiredMembers)
                    {
                        bool declared = requiredMember switch
                        {
                            "operator ==" => type.Members.OfType<OperatorDeclarationSyntax>().Any(op => op.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken)),
                            "operator !=" => type.Members.OfType<OperatorDeclarationSyntax>().Any(op => op.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)),
                            _ => type.Members.OfType<MethodDeclarationSyntax>().Any(method => string.Equals(method.Identifier.ValueText, requiredMember, StringComparison.Ordinal))
                        };

                        if (!declared)
                        {
                            violations.Add(ProductionSources.Describe(type, $"struct '{type.Identifier}' is missing '{requiredMember}'."));
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoPreviewLanguageFeatureIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                matched += 1;

                foreach (RecordDeclarationSyntax record in tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>())
                {
                    violations.Add(ProductionSources.Describe(record, "record declarations are prohibited."));
                }

                foreach (GlobalStatementSyntax global in tree.GetRoot().DescendantNodes().OfType<GlobalStatementSyntax>())
                {
                    violations.Add(ProductionSources.Describe(global, "top-level statements are prohibited."));
                }
            }

            ProductionSources.AssertClean("No preview language features", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoFileUsesFileScopedNamespace()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (FileScopedNamespaceDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>())
                {
                    matched += 1;
                    violations.Add(ProductionSources.Describe(declaration, "file-scoped namespace; use a block namespace."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenProgramDeclaresStaticVoidMain()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (!string.Equals(method.Identifier.ValueText, "Main", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        violations.Add(ProductionSources.Describe(method, "'Main' must be static."));
                    }

                    if (!string.Equals(method.ReturnType.ToString(), "void", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(method, "'Main' must return void."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        private static InterfaceDeclarationSyntax? FindInterface(SyntaxTree tree, string interfaceName)
            => tree.GetRoot()
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .FirstOrDefault(candidate => string.Equals(candidate.Identifier.ValueText, interfaceName, StringComparison.Ordinal));
    }
}