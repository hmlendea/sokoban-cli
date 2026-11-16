using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Resolves the handwritten source files of every checked project from explicit project metadata.
    /// </summary>
    internal static class ProductionSources
    {
        internal static readonly string[] ProductionProjectNames = [ "SokobanCLI" ];

        internal static readonly string[] TestProjectNames = [ "SokobanCLI.UnitTests" ];

        internal static readonly string[] CheckedProjectNames = [ "SokobanCLI", "SokobanCLI.UnitTests", "SokobanCLI.ArchitectureTests" ];

        internal static string RepositoryRoot => Path.GetFullPath(Path.Combine(ArchitecturePaths.ProductionSourceRoot, ".."));

        internal static IEnumerable<string> GetProductionFiles() => ProductionProjectNames.SelectMany(GetFiles);

        internal static IEnumerable<string> GetTestFiles() => TestProjectNames.SelectMany(GetFiles);

        internal static IEnumerable<string> GetCheckedFiles() => CheckedProjectNames.SelectMany(GetFiles);

        internal static IEnumerable<SyntaxTree> GetProductionTrees() => GetProductionFiles().Select(ParseFile);

        internal static IEnumerable<SyntaxTree> GetTestTrees() => GetTestFiles().Select(ParseFile);

        internal static string GetProjectDirectory(string projectName) => Path.GetFullPath(Path.Combine(RepositoryRoot, projectName));

        internal static string GetProjectFilePath(string projectName)
            => Path.Combine(GetProjectDirectory(projectName), projectName + ".csproj");

        internal static IEnumerable<string> GetFiles(string projectName)
        {
            string root = GetProjectDirectory(projectName);

            Assert.That(Directory.Exists(root), Is.True, $"The source directory '{root}' is unavailable.");

            return Directory
                .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(filePath => !IsUnderGeneratedDirectory(filePath))
                .Where(filePath => !IsGeneratedFile(filePath))
                .OrderBy(filePath => filePath, StringComparer.Ordinal);
        }

        internal static SyntaxTree ParseFile(string filePath)
            => CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath);

        internal static string Describe(SyntaxNode node, string message)
        {
            string filePath = node.SyntaxTree.FilePath;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "<unknown>";
            }

            int lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            return $"{filePath}:{lineNumber}: {message}";
        }

        internal static string DescribeFile(string filePath, int lineNumber, string message)
            => $"{filePath}:{lineNumber}: {message}";

        internal static void AssertClean(string rule, int matchedCount, IReadOnlyList<string> violations)
        {
            Assert.That(matchedCount, Is.GreaterThan(0), $"{rule}: the rule selected no source elements.");
            Assert.That(violations, Is.Empty, $"{rule}:{Environment.NewLine}{FormatViolations(violations)}");
        }

        // Prohibited-pattern rules expect zero occurrences. Vacuity is excluded by asserting that the
        // scanned production corpus is non-empty before the (necessarily empty) violation list.
        internal static void AssertScanned(
            IReadOnlyList<string> violations,
            [System.Runtime.CompilerServices.CallerMemberName] string rule = "")
        {
            Assert.That(GetProductionFiles().Count(), Is.GreaterThan(0), $"{rule}: no production sources were scanned.");
            Assert.That(violations, Is.Empty, $"{rule}:{Environment.NewLine}{FormatViolations(violations)}");
        }

        internal static string FormatViolations(IReadOnlyList<string> violations)
        {
            int limit = 20;

            if (violations.Count <= limit)
            {
                return string.Join(Environment.NewLine, violations);
            }

            return string.Join(Environment.NewLine, violations.Take(limit)) +
                Environment.NewLine +
                $"... and {violations.Count - limit} more.";
        }

        internal static int GetLineNumber(SyntaxNode node)
            => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

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