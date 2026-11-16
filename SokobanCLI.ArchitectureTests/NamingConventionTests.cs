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
    /// Enforces the naming rules of <c>csharp.naming.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class NamingConventionTests
    {
        private static readonly string[] BuiltInAliases =
        [
            "string", "int", "bool", "object", "long", "double", "float", "decimal",
            "byte", "char", "sbyte", "short", "ushort", "uint", "ulong", "nint", "nuint", "void"
        ];

        private static readonly Dictionary<string, string> BclTypeNames = new(StringComparer.Ordinal)
        {
            { "String", "string" },
            { "Int32", "int" },
            { "UInt32", "uint" },
            { "Int64", "long" },
            { "UInt64", "ulong" },
            { "Int16", "short" },
            { "UInt16", "ushort" },
            { "Boolean", "bool" },
            { "Object", "object" },
            { "Double", "double" },
            { "Single", "float" },
            { "Decimal", "decimal" },
            { "Byte", "byte" },
            { "SByte", "sbyte" },
            { "Char", "char" },
            { "Void", "void" }
        };

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoBclTypeNameIsUsedInsteadOfTheAlias()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (PredefinedTypeSyntax predefined in tree.GetRoot().DescendantNodes().OfType<PredefinedTypeSyntax>())
                {
                    matched += 1;

                    if (!BuiltInAliases.Contains(predefined.Keyword.ValueText, StringComparer.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(predefined, $"'{predefined.Keyword.ValueText}' is not an allowed alias."));
                    }
                }

                foreach (IdentifierNameSyntax identifier in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    matched += 1;

                    if (BclTypeNames.TryGetValue(identifier.Identifier.ValueText, out string? alias) &&
                        IsTypePosition(identifier))
                    {
                        violations.Add(ProductionSources.Describe(identifier, $"'{identifier.Identifier.ValueText}' must be written as '{alias}'."));
                    }
                }
            }

            ProductionSources.AssertClean("Built-in type aliases", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenPublicAndProtectedMembersStartUppercase()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();

                foreach (MemberDeclarationSyntax member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    string? name = GetDeclaredName(member);

                    if (name is null || !IsUppercase(name))
                    {
                        if (name is not null)
                        {
                            matched += 1;
                            violations.Add(ProductionSources.Describe(member, $"public/protected member '{name}' does not start uppercase."));
                        }

                        continue;
                    }

                    if (IsPublicOrProtected(member))
                    {
                        matched += 1;
                    }
                }
            }

            ProductionSources.AssertClean("Public/protected members start uppercase", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenClassesAndMethodsArePascalCase()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    matched += 1;

                    if (!IsPascalCase(type.Identifier.ValueText))
                    {
                        violations.Add(ProductionSources.Describe(type, $"class '{type.Identifier.ValueText}' is not PascalCase."));
                    }
                }

                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    matched += 1;

                    if (!IsPascalCase(method.Identifier.ValueText))
                    {
                        violations.Add(ProductionSources.Describe(method, $"method '{method.Identifier.ValueText}' is not PascalCase."));
                    }
                }
            }

            ProductionSources.AssertClean("PascalCase classes and methods", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenPrivateFieldsUseCamelCaseWithoutUnderscorePrefix()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (FieldDeclarationSyntax field in tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    if (!field.Modifiers.Any(SyntaxKind.PrivateKeyword) &&
                        !HasNoAccessibility(field.Modifiers))
                    {
                        continue;
                    }

                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        matched += 1;
                        string name = variable.Identifier.ValueText;

                        if (name.StartsWith('_'))
                        {
                            violations.Add(ProductionSources.Describe(variable, $"private field '{name}' uses an underscore prefix."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("Private fields without underscore prefix", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenBooleanMembersUsePredicatePrefixes()
        {
            string[] prefixes = [ "Is", "Has", "Does", "Are", "Was", "Were", "Can", "Should" ];
            string[] forbidden = [ "flag", "check", "result" ];

            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (PropertyDeclarationSyntax property in tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    if (!string.Equals(property.Type.ToString(), "bool", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string name = property.Identifier.ValueText;

                    if (forbidden.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        violations.Add(ProductionSources.Describe(property, $"boolean '{name}' uses a forbidden generic name."));
                    }
                    else if (!prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                    {
                        violations.Add(ProductionSources.Describe(property, $"boolean '{name}' lacks a predicate prefix."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenInterfacesAreIPrefixedPascalCase()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (InterfaceDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                {
                    matched += 1;
                    string name = declaration.Identifier.ValueText;

                    if (!name.StartsWith('I') || name.Length < 2 || !char.IsUpper(name[1]))
                    {
                        violations.Add(ProductionSources.Describe(declaration, $"interface '{name}' must be 'I' + PascalCase."));
                    }
                }
            }

            ProductionSources.AssertClean("Interface naming", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenConfigurationClassesUseSettingsSuffix()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    string namespaceName = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

                    if (!namespaceName.EndsWith(".Configuration", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    if (!type.Identifier.ValueText.EndsWith("Settings", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(type, $"configuration class '{type.Identifier.ValueText}' must end with 'Settings'."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenDomainModelNamesArePlainNouns()
        {
            string[] prohibitedSuffixes = [ "DataObject", "Request", "Response", "Settings", "Extensions", "Manager", "Service", "Factory", "Helper" ];

            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    string namespaceName = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

                    if (!string.Equals(namespaceName, "SokobanCLI.Models", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    foreach (string suffix in prohibitedSuffixes)
                    {
                        if (type.Identifier.ValueText.EndsWith(suffix, StringComparison.Ordinal))
                        {
                            violations.Add(ProductionSources.Describe(type, $"domain model '{type.Identifier.ValueText}' must not use the suffix '{suffix}'."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("Domain model naming", matched, violations);
        }

        private static bool IsUppercase(string name) => name.Length > 0 && char.IsUpper(name[0]);

        private static bool IsPascalCase(string name)
            => name.Length > 0 && char.IsUpper(name[0]) && !name.Contains('_', StringComparison.Ordinal);

        private static bool HasNoAccessibility(SyntaxTokenList modifiers)
            => !modifiers.Any(modifier =>
                modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.PrivateKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword) ||
                modifier.IsKind(SyntaxKind.InternalKeyword));

        private static bool IsPublicOrProtected(MemberDeclarationSyntax member)
        {
            SyntaxTokenList modifiers = member switch
            {
                BaseTypeDeclarationSyntax type => type.Modifiers,
                BaseMethodDeclarationSyntax method => method.Modifiers,
                BasePropertyDeclarationSyntax property => property.Modifiers,
                BaseFieldDeclarationSyntax field => field.Modifiers,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Modifiers,
                _ => default
            };

            return modifiers.Any(modifier =>
                modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword));
        }

        private static string? GetDeclaredName(MemberDeclarationSyntax member)
            => member switch
            {
                BaseTypeDeclarationSyntax type when IsPublicOrProtected(member) => type.Identifier.ValueText,
                MethodDeclarationSyntax method => method.Identifier.ValueText,
                PropertyDeclarationSyntax property => property.Identifier.ValueText,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.ValueText,
                EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.ValueText,
                _ => null
            };

        private static bool IsTypePosition(IdentifierNameSyntax identifier)
            => identifier.Parent is VariableDeclarationSyntax ||
                identifier.Parent is ParameterSyntax ||
                identifier.Parent is MethodDeclarationSyntax ||
                identifier.Parent is PropertyDeclarationSyntax ||
                identifier.Parent is ObjectCreationExpressionSyntax;
    }
}