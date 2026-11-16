using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Enforces the source-file identity conventions on the handwritten production sources.
    /// </summary>
    [TestFixture]
    public sealed class SourceFileConventionTests
    {
        private static readonly string[] CheckedProjectDirectories =
        [
            "SokobanCLI",
            "SokobanCLI.UnitTests"
        ];

        [Test]
        public void GivenTheCheckedProjects_WhenEnumerated_ThenHandwrittenSourcesAreAvailable()
        {
            List<string> files = GetSourceFiles("SokobanCLI").ToList();

            Assert.That(files, Is.Not.Empty, "No production source files were found.");
            Assert.That(GetSourceFiles("SokobanCLI.UnitTests"), Is.Not.Empty, "No test source files were found.");
        }

        [Test]
        public void GivenEveryProductionSourceFile_WhenParsed_ThenItDeclaresExactlyOneTypeAndMatchesItsFileName()
        {
            List<string> failures = [];

            foreach (string filePath in GetSourceFiles("SokobanCLI"))
            {
                SyntaxNode root = Parse(filePath);
                List<string> declaredNames = root
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
                    .Select(node => node.Identifier.ValueText)
                    .ToList();

                declaredNames.AddRange(root
                    .DescendantNodes()
                    .OfType<DelegateDeclarationSyntax>()
                    .Select(node => node.Identifier.ValueText));

                string expectedName = Path.GetFileNameWithoutExtension(filePath);

                if (declaredNames.Count != 1)
                {
                    failures.Add($"{filePath}: declares {declaredNames.Count} types ({string.Join(", ", declaredNames)}).");
                    continue;
                }

                if (!string.Equals(declaredNames[0], expectedName, StringComparison.Ordinal))
                {
                    failures.Add($"{filePath}: type '{declaredNames[0]}' must match the filename '{expectedName}'.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenParsed_ThenItsFilenameStemMatchesItsTypeName()
        {
            List<string> failures = [];

            foreach (string projectDirectory in CheckedProjectDirectories)
            {
                foreach (string filePath in GetSourceFiles(projectDirectory))
                {
                    SyntaxNode root = Parse(filePath);
                    string expectedStem = Path.GetFileNameWithoutExtension(filePath);

                    List<string> declaredNames = root
                        .DescendantNodes()
                        .OfType<BaseTypeDeclarationSyntax>()
                        .Select(node => node.Identifier.ValueText)
                        .ToList();

                    declaredNames.AddRange(root
                        .DescendantNodes()
                        .OfType<DelegateDeclarationSyntax>()
                        .Select(node => node.Identifier.ValueText));

                    if (declaredNames.Any(name => !string.Equals(name, expectedStem, StringComparison.Ordinal)))
                    {
                        failures.Add($"{filePath}: declares {string.Join(", ", declaredNames)}.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenParsed_ThenItDeclaresOneBlockNamespaceAndNoFileScopedNamespace()
        {
            List<string> failures = [];

            foreach (string filePath in GetCheckedSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                if (root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Any())
                {
                    failures.Add($"{filePath}: uses a file-scoped namespace.");
                }

                int blockNamespaceCount = root
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Count();

                if (blockNamespaceCount != 1)
                {
                    failures.Add($"{filePath}: declares {blockNamespaceCount} block namespaces.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenParsed_ThenItsNamespaceMatchesItsFolder()
        {
            List<string> failures = [];

            foreach (string filePath in GetCheckedSourceFiles())
            {
                string expectedNamespace = GetExpectedNamespace(filePath);
                SyntaxNode root = Parse(filePath);

                List<string> declaredNamespaces = root
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Select(node => node.Name.ToString())
                    .ToList();

                if (declaredNamespaces.Count == 1 &&
                    !string.Equals(declaredNamespaces[0], expectedNamespace, StringComparison.Ordinal))
                {
                    failures.Add($"{filePath}: namespace '{declaredNamespaces[0]}' must equal '{expectedNamespace}'.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenInspected_ThenItContainsNoRegionsOrNullableDirectives()
        {
            List<string> failures = [];

            foreach (string filePath in GetCheckedSourceFiles())
            {
                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmedLine = line.TrimStart();

                    if (trimmedLine.StartsWith("#region", StringComparison.Ordinal) ||
                        trimmedLine.StartsWith("#endregion", StringComparison.Ordinal) ||
                        trimmedLine.StartsWith("#nullable", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: contains the directive '{trimmedLine}'.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenInspected_ThenItContainsNoTopLevelStatements()
        {
            List<string> failures = [];

            foreach (string filePath in GetCheckedSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                if (root.ChildNodes().OfType<GlobalStatementSyntax>().Any())
                {
                    failures.Add($"{filePath}: contains top-level statements.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedSourceFile_WhenInspected_ThenItEndsWithASingleTrailingNewline()
        {
            List<string> failures = [];

            foreach (string filePath in GetCheckedSourceFiles())
            {
                string content = File.ReadAllText(filePath);

                if (!content.EndsWith('\n'))
                {
                    failures.Add($"{filePath}: does not end with a newline.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        private static IEnumerable<string> GetCheckedSourceFiles()
            => CheckedProjectDirectories.SelectMany(GetSourceFiles);

        private static IEnumerable<string> GetSourceFiles(string projectDirectory)
        {
            string root = Path.GetFullPath(Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", projectDirectory));

            Assert.That(Directory.Exists(root), Is.True, $"The source root '{root}' is unavailable.");

            return Directory
                .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(filePath => !IsUnderGeneratedDirectory(filePath))
                .Where(filePath => !IsGeneratedFile(filePath))
                .OrderBy(filePath => filePath, StringComparer.Ordinal);
        }

        private static SyntaxNode Parse(string filePath)
            => CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath).GetRoot();

        private static string GetExpectedNamespace(string filePath)
        {
            string projectDirectory = CheckedProjectDirectories
                .Select(directory => Path.GetFullPath(Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", directory)))
                .First(directory => filePath.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.Ordinal));

            string relativeDirectory = Path.GetDirectoryName(filePath)!
                .Substring(projectDirectory.Length)
                .Trim(Path.DirectorySeparatorChar);

            string projectName = Path.GetFileName(projectDirectory);

            if (string.IsNullOrEmpty(relativeDirectory))
            {
                return projectName;
            }

            return projectName + "." + relativeDirectory.Replace(Path.DirectorySeparatorChar, '.');
        }

        private static bool IsUnderGeneratedDirectory(string filePath)
        {
            string normalisedPath = filePath.Replace('\\', '/');

            return normalisedPath.Contains("/bin/", StringComparison.Ordinal) ||
                normalisedPath.Contains("/obj/", StringComparison.Ordinal);
        }

        private static bool IsGeneratedFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            return fileName.EndsWith(".g.cs", StringComparison.Ordinal) ||
                fileName.EndsWith(".Designer.cs", StringComparison.Ordinal) ||
                fileName.EndsWith(".AssemblyInfo.cs", StringComparison.Ordinal);
        }
    }
}