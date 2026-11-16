using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Verifies the project- and assembly-level reference graph independently of the compiled type dependencies.
    /// </summary>
    [TestFixture]
    public sealed class ProjectReferenceTests
    {
        private static readonly Assembly ProductionAssembly = typeof(SokobanCLI.Models.World).Assembly;

        private static string ProductionProjectPath
            => Path.Combine(ArchitecturePaths.ProductionSourceRoot, "SokobanCLI.csproj");

        private static readonly string[] ForbiddenPackageFragments =
        [
            "NUnit",
            "Moq",
            "Test.Sdk",
            "TestAdapter",
            "xunit",
            "ArchUnitNET",
            "EntityFramework",
            "Dapper",
            "SqlClient",
            "Npgsql",
            "Dynamo"
        ];

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDeclaresNoProjectReference()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);

            List<string> references = project
                .Descendants("ProjectReference")
                .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
                .ToList();

            Assert.That(references, Is.Empty);
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDeclaresNoTestOrDatabasePackage()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);

            List<string> forbiddenPackages = project
                .Descendants("PackageReference")
                .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
                .Where(name => ForbiddenPackageFragments.Any(fragment =>
                    name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Assert.That(forbiddenPackages, Is.Empty);
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDeclaresNoInternalsVisibleTo()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);

            List<string> exposures = project
                .Descendants("InternalsVisibleTo")
                .Select(element => element.Value)
                .ToList();

            exposures.AddRange(project
                .Descendants("AssemblyAttribute")
                .Where(attribute => ((string?)attribute.Attribute("Include") ?? string.Empty)
                    .Contains("InternalsVisibleTo", StringComparison.Ordinal))
                .Select(attribute => attribute.Value));

            Assert.That(exposures, Is.Empty);
        }

        [Test]
        public void GivenTheCompiledProductionAssembly_WhenInspected_ThenItReferencesNoTestOrDatabaseAssembly()
        {
            List<string> forbiddenReferences = ProductionAssembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .Where(name => ForbiddenPackageFragments.Any(fragment =>
                    name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Assert.That(forbiddenReferences, Is.Empty);
        }

        [Test]
        public void GivenTheArchitectureTestProject_WhenInspected_ThenItIsTheOnlyConsumerOfTheProductionProject()
        {
            string root = Path.GetFullPath(Path.Combine(ArchitecturePaths.ProductionSourceRoot, ".."));

            List<string> consumers = Directory
                .EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
                .Where(file => !string.Equals(file, ProductionProjectPath, StringComparison.Ordinal))
                .Where(file => XDocument.Load(file)
                    .Descendants("ProjectReference")
                    .Any(reference => ((string?)reference.Attribute("Include") ?? string.Empty)
                        .Contains("SokobanCLI.csproj", StringComparison.Ordinal)))
                .Select(file => Path.GetFileName(file) ?? string.Empty)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            Assert.That(consumers, Is.EqualTo(new[] { "SokobanCLI.ArchitectureTests.csproj", "SokobanCLI.UnitTests.csproj" }));
        }
    }
}