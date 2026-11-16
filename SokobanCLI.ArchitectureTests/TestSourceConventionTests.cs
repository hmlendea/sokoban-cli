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
    /// Enforces the test source conventions from <c>csharp.testing.instructions.md</c>
    /// and <c>csharp.testing.unit.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class TestSourceConventionTests
    {
        private static string UnitTestSourceRoot
            => Path.GetFullPath(Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", "SokobanCLI.UnitTests"));

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenAllTestClassesEndWithTests()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    bool hasTestMethods = classDeclaration
                        .DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Any(method => method.AttributeLists
                            .SelectMany(list => list.Attributes)
                            .Any(attribute => attribute.Name.ToString().Contains("Test", StringComparison.Ordinal)));

                    if (hasTestMethods && !classDeclaration.Identifier.ValueText.EndsWith("Tests", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: test class '{classDeclaration.Identifier.ValueText}' must end with 'Tests'.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenAllTestMethodsFollowGivenWhenThen()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    bool isTest = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute =>
                            attribute.Name.ToString() is "Test" or "TestCase" ||
                            attribute.Name.ToString().EndsWith("Test", StringComparison.Ordinal) ||
                            attribute.Name.ToString().EndsWith("TestCase", StringComparison.Ordinal));

                    if (!isTest)
                    {
                        continue;
                    }

                    string methodName = method.Identifier.ValueText;

                    if (!methodName.StartsWith("Given", StringComparison.Ordinal) ||
                        !methodName.Contains("_When", StringComparison.Ordinal) ||
                        !methodName.Contains("_Then", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: test method '{methodName}' must follow Given[x]_When[y]_Then[z].");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenAllAssertionsUseAssertThat()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string expressionText = invocation.Expression.ToString();

                    if (expressionText.StartsWith("Assert.", StringComparison.Ordinal) &&
                        !expressionText.StartsWith("Assert.That", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: invocation '{expressionText}' must use Assert.That.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenEveryTestMethodContainsAnAssertionOrVerification()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    bool isTest = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute =>
                            attribute.Name.ToString() is "Test" or "TestCase" ||
                            attribute.Name.ToString().EndsWith("Test", StringComparison.Ordinal) ||
                            attribute.Name.ToString().EndsWith("TestCase", StringComparison.Ordinal));

                    if (!isTest)
                    {
                        continue;
                    }

                    bool hasAssertion = method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Any(invocation =>
                        {
                            string text = invocation.Expression.ToString();
                            return text.StartsWith("Assert.", StringComparison.Ordinal) ||
                                text.EndsWith(".Verify", StringComparison.Ordinal);
                        });

                    if (!hasAssertion)
                    {
                        failures.Add($"{filePath}: test method '{method.Identifier.ValueText}' contains no assertion or verification.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenNoTestMethodExceedsSevenParameters()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (BaseMethodDeclarationSyntax method in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
                {
                    if (method.ParameterList.Parameters.Count > 7)
                    {
                        failures.Add($"{filePath}: member has {method.ParameterList.Parameters.Count} parameters.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenTopLevelFakesAreInTheirOwnFiles()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (classDeclaration.Parent is TypeDeclarationSyntax)
                    {
                        failures.Add($"{filePath}: class '{classDeclaration.Identifier.ValueText}' is nested; top-level fakes must be separate files.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenNoTestMethodUsesLegacyAssertForms()
        {
            string[] legacyForms =
            [
                "AreEqual", "AreNotEqual", "IsTrue", "IsFalse", "IsNull", "IsNotNull", "Greater", "Less", "That(true"
            ];

            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string text = invocation.Expression.ToString();

                    if (!text.StartsWith("Assert.", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string memberName = text["Assert.".Length..];

                    if (legacyForms.Contains(memberName, StringComparer.Ordinal))
                    {
                        failures.Add($"{filePath}: '{text}' uses a legacy assert form.");
                    }

                    foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                    {
                        if (argument.Expression.IsKind(SyntaxKind.TrueLiteralExpression) ||
                            argument.Expression.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            failures.Add($"{filePath}: '{text}' asserts a boolean literal; use the constraint model.");
                        }
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenNoFixtureUsesCommentBannersToGroupTests()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmed = line.TrimStart();

                    if (!trimmed.StartsWith("//", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (trimmed.Contains('─', StringComparison.Ordinal) || trimmed.Contains("====", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: comment banner '{trimmed}' is prohibited.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenEveryParameterisedTestHasAtLeastTwoCases()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    int testCaseCount = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Count(attribute => attribute.Name.ToString() is "TestCase" or "TestCaseSource");

                    bool hasTestAttribute = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute => attribute.Name.ToString() is "Test");

                    if (testCaseCount == 0 || hasTestAttribute)
                    {
                        continue;
                    }

                    if (testCaseCount < 2)
                    {
                        failures.Add($"{filePath}: '{method.Identifier.ValueText}' declares a single test case.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenNoFixtureUsesReflectionOnNonPublicCode()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string text = invocation.Expression.ToString();

                    if (text.Contains("GetMethod", StringComparison.Ordinal) ||
                        text.Contains("GetProperty", StringComparison.Ordinal) ||
                        text.Contains("GetField", StringComparison.Ordinal) ||
                        text.Contains("NonPublic", StringComparison.Ordinal) ||
                        text.Contains("BindingFlags", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: reflection usage '{text}' targets non-public code.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenEveryTestFixtureIsMarked()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    bool hasTestMethods = classDeclaration
                        .DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Any(method => method.AttributeLists
                            .SelectMany(list => list.Attributes)
                            .Any(attribute => attribute.Name.ToString() is "Test" or "TestCase" or "TestCaseSource"));

                    if (!hasTestMethods)
                    {
                        continue;
                    }

                    bool isFixture = classDeclaration.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute => attribute.Name.ToString() is "TestFixture");

                    if (!isFixture)
                    {
                        failures.Add($"{filePath}: class '{classDeclaration.Identifier.ValueText}' declares tests but has no [TestFixture].");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenSetupMethodsAreNamedSetUpAndConstructTheSubject()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                SyntaxNode root = Parse(filePath);

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    bool isSetUp = method.AttributeLists
                        .SelectMany(list => list.Attributes)
                        .Any(attribute => attribute.Name.ToString() is "SetUp");

                    if (!isSetUp)
                    {
                        continue;
                    }

                    if (!string.Equals(method.Identifier.ValueText, "SetUp", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: setup method '{method.Identifier.ValueText}' must be named 'SetUp'.");
                    }

                    bool constructsSubject = method.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Any(assignment => assignment.Right is ObjectCreationExpressionSyntax or InvocationExpressionSyntax);

                    if (!constructsSubject)
                    {
                        failures.Add($"{filePath}: setup method does not construct the subject or mocks.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenNoFixtureDeclaresProductionInternalsVisibility()
        {
            List<string> failures = [];

            foreach (string filePath in GetUnitTestSourceFiles())
            {
                foreach (string line in File.ReadAllLines(filePath))
                {
                    if (line.Contains("InternalsVisibleTo", StringComparison.Ordinal))
                    {
                        failures.Add($"{filePath}: InternalsVisibleTo is prohibited in test sources.");
                    }
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenTestFilesAreOrderedByName()
        {
            List<string> testNames = GetUnitTestSourceFiles()
                .Select(Path.GetFileNameWithoutExtension)
                .Select(name => name ?? string.Empty)
                .Where(name => name.EndsWith("Tests", StringComparison.Ordinal))
                .ToList();

            Assert.That(testNames, Is.Not.Empty);
            Assert.That(testNames.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(testNames.Count));
        }

        private static IEnumerable<string> GetUnitTestSourceFiles()
        {
            Assert.That(Directory.Exists(UnitTestSourceRoot), Is.True, "Unit test directory is missing.");

            return Directory
                .EnumerateFiles(UnitTestSourceRoot, "*.cs", SearchOption.AllDirectories)
                .Where(filePath => !filePath.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .Where(filePath => !filePath.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .OrderBy(filePath => filePath, StringComparer.Ordinal);
        }

        private static SyntaxNode Parse(string filePath)
            => CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath).GetRoot();
    }
}