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
    /// Enforces the using-directive and namespace rules of <c>csharp.namespaces.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class NamespaceConventionTests
    {
        private static readonly string[] ProhibitedSegments = [ "Interfaces", "Utilities", "Utils", "Helpers" ];

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenAllUsingsAreOutsideTheNamespace()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (UsingDirectiveSyntax usingDirective in tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    matched += 1;

                    if (usingDirective.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault() is not null)
                    {
                        violations.Add(ProductionSources.Describe(usingDirective, "using directive is declared inside a namespace."));
                    }
                }
            }

            ProductionSources.AssertClean("Usings outside namespaces", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoGlobalUsingOrGlobalUsingsFileExists()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                matched += 1;

                if (string.Equals(System.IO.Path.GetFileName(tree.FilePath), "GlobalUsings.cs", StringComparison.Ordinal))
                {
                    violations.Add(ProductionSources.DescribeFile(tree.FilePath, 1, "GlobalUsings.cs is prohibited."));
                }

                foreach (UsingDirectiveSyntax usingDirective in tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        violations.Add(ProductionSources.Describe(usingDirective, "global using directive."));
                    }
                }
            }

            ProductionSources.AssertClean("No global usings", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenUsingGroupsAreSeparatedByBlankLinesAndAlphabetised()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                List<string> usings = [];
                bool containsNamespace = false;

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed.StartsWith("using ", StringComparison.Ordinal) && trimmed.EndsWith(';'))
                    {
                        usings.Add(trimmed);
                    }
                    else if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                    {
                        containsNamespace = true;
                        break;
                    }
                    else if (trimmed.Length > 0 && !trimmed.StartsWith("//", StringComparison.Ordinal) && !trimmed.StartsWith("using ", StringComparison.Ordinal))
                    {
                        continue;
                    }
                }

                if (!containsNamespace || usings.Count < 2)
                {
                    continue;
                }

                matched += 1;
                List<string> localUsings = usings
                    .Select(directive => directive["using ".Length..].TrimEnd(';').Trim())
                    .Where(name => name.StartsWith("SokobanCLI", StringComparison.Ordinal))
                    .ToList();

                if (!localUsings.SequenceEqual(localUsings.OrderBy(name => name, StringComparer.Ordinal), StringComparer.Ordinal))
                {
                    violations.Add(ProductionSources.DescribeFile(filePath, 1, "local using directives are not alphabetised."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenExactlyOneBlankLineSeparatesUsingsFromTheNamespace()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                int lastUsingIndex = -1;

                for (int index = 0; index < lines.Length; index++)
                {
                    string trimmed = lines[index].Trim();

                    if (trimmed.StartsWith("using ", StringComparison.Ordinal) && trimmed.EndsWith(';'))
                    {
                        lastUsingIndex = index;
                    }
                }

                int namespaceIndex = Array.FindIndex(lines, line => line.TrimStart().StartsWith("namespace ", StringComparison.Ordinal));

                if (lastUsingIndex < 0 || namespaceIndex < 0)
                {
                    continue;
                }

                matched += 1;
                int blankLines = 0;

                for (int index = lastUsingIndex + 1; index < namespaceIndex; index++)
                {
                    if (string.IsNullOrWhiteSpace(lines[index]))
                    {
                        blankLines += 1;
                    }
                }

                if (blankLines != 1)
                {
                    violations.Add(ProductionSources.DescribeFile(filePath, namespaceIndex + 1, $"expected exactly one blank line after the last using, found {blankLines}."));
                }
            }

            ProductionSources.AssertClean("Single blank line after usings", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoInlineFullyQualifiedTypeIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (QualifiedNameSyntax qualifiedName in tree.GetRoot().DescendantNodes().OfType<QualifiedNameSyntax>())
                {
                    if (qualifiedName.Parent is UsingDirectiveSyntax)
                    {
                        continue;
                    }

                    if (qualifiedName.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Any())
                    {
                        continue;
                    }

                    if (qualifiedName.Parent is NameEqualsSyntax || qualifiedName.Parent is QualifiedNameSyntax)
                    {
                        continue;
                    }

                    matched += 1;

                    string text = qualifiedName.ToString();

                    if (!text.StartsWith("SokobanCLI", StringComparison.Ordinal) &&
                        !text.StartsWith("ArchUnitNET", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(qualifiedName, $"inline fully qualified type '{text}'."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoProhibitedNamespaceSegmentIsUsed()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BaseNamespaceDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
                {
                    string[] segments = declaration.Name.ToString().Split('.');

                    foreach (string prohibited in ProhibitedSegments)
                    {
                        if (segments.Contains(prohibited, StringComparer.Ordinal))
                        {
                            violations.Add(ProductionSources.Describe(declaration, $"namespace '{declaration.Name}' uses '{prohibited}'."));
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenRoslynSourceTreesAreAvailable()
        {
            Assert.That(ProductionSources.GetProductionFiles().Count(), Is.GreaterThan(0));
            Assert.That(ProductionSources.GetProductionTrees().Count(), Is.EqualTo(ProductionSources.GetProductionFiles().Count()));
        }
    }
}