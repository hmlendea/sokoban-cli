using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Verifies the project-file conventions that the current projects already satisfy.
    /// </summary>
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        private static string ProductionProjectPath
            => Path.Combine(ArchitecturePaths.ProductionSourceRoot, "SokobanCLI.csproj");

        private static string UnitTestProjectPath
            => Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", "SokobanCLI.UnitTests", "SokobanCLI.UnitTests.csproj");

        private static string ArchitectureTestProjectPath
            => Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", "SokobanCLI.ArchitectureTests", "SokobanCLI.ArchitectureTests.csproj");

        [Test]
        public void GivenEveryProject_WhenInspected_ThenItTargetsNetTen()
        {
            foreach (string projectPath in GetProjectPaths())
            {
                XDocument project = XDocument.Load(projectPath);

                Assert.That(
                    project.Descendants("TargetFramework").FirstOrDefault()?.Value,
                    Is.EqualTo("net10.0"),
                    $"{Path.GetFileName(projectPath)} must target net10.0.");
            }
        }

        [TestCase("SokobanCLI")]
        [TestCase("SokobanCLI.UnitTests")]
        [TestCase("SokobanCLI.ArchitectureTests")]
        public void GivenAProject_WhenInspected_ThenItsPackageReferencesAreAlphabetised(string projectName)
        {
            XDocument project = XDocument.Load(GetProjectPath(projectName));

            List<string> packageNames = project
                .Descendants("PackageReference")
                .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
                .ToList();

            Assert.That(packageNames, Is.Not.Empty, $"{projectName} must declare its dependencies explicitly.");
            Assert.That(packageNames, Is.EqualTo(packageNames.OrderBy(name => name, StringComparer.Ordinal).ToList()));
        }

        [TestCase("SokobanCLI.UnitTests")]
        [TestCase("SokobanCLI.ArchitectureTests")]
        public void GivenATestProject_WhenInspected_ThenItIsNotPackable(string projectName)
        {
            XDocument project = XDocument.Load(GetProjectPath(projectName));

            Assert.That(
                project.Descendants("IsPackable").FirstOrDefault()?.Value,
                Is.EqualTo("false"),
                $"{projectName} must not be packable.");
        }

        [Test]
        public void GivenTheArchitectureTestProject_WhenInspected_ThenItDeclaresTheTestProjectFlags()
        {
            XDocument project = XDocument.Load(ArchitectureTestProjectPath);

            Assert.That(project.Descendants("IsTestProject").FirstOrDefault()?.Value, Is.EqualTo("true"));
            Assert.That(project.Descendants("ExcludeFromCodeCoverage").FirstOrDefault()?.Value, Is.EqualTo("true"));
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDeclaresTheLevelFilesAsContent()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);

            List<string> levelFiles = project
                .Descendants("None")
                .Select(element => (string?)element.Attribute("Update") ?? string.Empty)
                .Where(path => path.StartsWith("Levels", StringComparison.Ordinal))
                .ToList();

            Assert.That(levelFiles, Is.Not.Empty);
        }

        private static IEnumerable<string> GetProjectPaths()
            => new[] { ProductionProjectPath, UnitTestProjectPath, ArchitectureTestProjectPath };

        private static string GetProjectPath(string projectName)
            => Path.Combine(ArchitecturePaths.ProductionSourceRoot, "..", projectName, projectName + ".csproj");
    }
}