using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Records which instruction families cannot be enforced statically for this repository,
    /// with the structural precondition that keeps them inapplicable. Each check fails once the
    /// precondition changes, forcing the corresponding rule to be written instead of skipped.
    /// </summary>
    [TestFixture]
    public sealed class RuleApplicabilityTests
    {
        [Test]
        public void GivenTheProductionSources_WhenInspected_ThenNoLoggingOrTelemetryCodeExists()
        {
            string[] loggingFragments = [ "NuciLog", "ILogger", "LogInformation", "LogError", "TelemetryClient" ];

            List<string> matches = ProductionSources
                .GetProductionFiles()
                .SelectMany(file => System.IO.File.ReadAllLines(file))
                .Where(line => loggingFragments.Any(fragment => line.Contains(fragment, StringComparison.Ordinal)))
                .ToList();

            Assert.That(matches, Is.Empty, "Logging rules (Operation, LogInfoKey, masking) become enforceable once logging code exists.");
        }

        [Test]
        public void GivenTheProductionSources_WhenInspected_ThenNoHttpOrNetworkCodeExists()
        {
            string[] networkFragments = [ "HttpClient", "NuciWeb", "HttpRequestMessage", "IPAddress" ];

            List<string> matches = ProductionSources
                .GetProductionFiles()
                .SelectMany(file => System.IO.File.ReadAllLines(file))
                .Where(line => networkFragments.Any(fragment => line.Contains(fragment, StringComparison.Ordinal)))
                .ToList();

            Assert.That(matches, Is.Empty, "NuciWeb.HTTP and listener rules become enforceable once network code exists.");
        }

        [Test]
        public void GivenTheProductionSources_WhenInspected_ThenNoGameFrameworkRenderingApiIsUsed()
        {
            string[] frameworkFragments = [ "GuiImage", "GuiText", "GuiControl", "Texture2D", "SpriteFont" ];

            List<string> matches = ProductionSources
                .GetProductionFiles()
                .SelectMany(file => System.IO.File.ReadAllLines(file))
                .Where(line => frameworkFragments.Any(fragment => line.Contains(fragment, StringComparison.Ordinal)))
                .ToList();

            Assert.That(matches, Is.Empty, "MonoGame/NuciXNA rules become enforceable once the GUI framework is referenced.");
        }

        [Test]
        public void GivenTheRepository_WhenInspected_ThenNoAnalyserConfigurationFileExists()
        {
            List<string> configurationFiles = System.IO.Directory
                .EnumerateFiles(ProductionSources.RepositoryRoot, "*", System.IO.SearchOption.TopDirectoryOnly)
                .Select(System.IO.Path.GetFileName)
                .Select(name => name ?? string.Empty)
                .Where(name =>
                    string.Equals(name, ".editorconfig", StringComparison.Ordinal) ||
                    string.Equals(name, "Directory.Build.props", StringComparison.Ordinal) ||
                    name.EndsWith(".ruleset", StringComparison.Ordinal))
                .ToList();

            Assert.That(configurationFiles, Is.Empty, "Cognitive-complexity and analyser thresholds are enforced by the equivalence rule below once configured.");
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItIsNotPublishedAsANuGetPackage()
        {
            XDocument project = XDocument.Load(ProductionSources.GetProjectFilePath("SokobanCLI"));

            bool isPackable = string.Equals(
                project.Descendants("IsPackable").FirstOrDefault()?.Value,
                "true",
                StringComparison.OrdinalIgnoreCase);

            Assert.That(isPackable, Is.False, "NuGet XML documentation and public-API baseline rules become enforceable once the project is packable.");
        }

        [Test]
        public void GivenTheProductionSources_WhenInspected_ThenNoInteropDynamoOrDatabaseCodeExists()
        {
            string[] persistenceFragments = [ "EntityFramework", "Dapper", "DynamoDB", "SqlConnection", "Npgsql" ];

            List<string> matches = ProductionSources
                .GetProductionFiles()
                .SelectMany(file => System.IO.File.ReadAllLines(file))
                .Where(line => persistenceFragments.Any(fragment => line.Contains(fragment, StringComparison.Ordinal)))
                .ToList();

            Assert.That(matches, Is.Empty, "Persistence-layer boundary rules become enforceable once a database provider is referenced.");
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDeclaresNoGeneratedRegexOrPartialTypePrecondition()
        {
            List<string> partialDeclarations = ProductionSources
                .GetProductionTrees()
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
                .Where(type => type.Modifiers.Any(SyntaxKind.PartialKeyword))
                .Select(type => type.Identifier.ValueText)
                .ToList();

            Assert.That(partialDeclarations, Is.Empty, "The partial-type prohibition may only be relaxed once GeneratedRegex is introduced.");
        }

        [Test]
        public void GivenTheUnitTestProject_WhenInspected_ThenItUsesTheDefaultFrameworkPair()
        {
            XDocument project = XDocument.Load(ProductionSources.GetProjectFilePath("SokobanCLI.UnitTests"));

            List<string> packages = project
                .Descendants("PackageReference")
                .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
                .ToList();

            Assert.That(packages, Does.Contain("NUnit"));
            Assert.That(packages, Does.Contain("Moq"));
            Assert.That(packages.Any(name => name.Contains("xunit", StringComparison.OrdinalIgnoreCase)), Is.False);
        }
    }
}