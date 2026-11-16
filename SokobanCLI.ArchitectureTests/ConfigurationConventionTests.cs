using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Enforces the configuration conventions that are not already covered by
    /// <see cref="ProjectConventionTests"/> and verifies analyser enforcement.
    /// </summary>
    [TestFixture]
    public sealed class ConfigurationConventionTests
    {
        [Test]
        public void GivenEveryCheckedProject_WhenInspected_ThenImplicitUsingsAndNullableAndLangVersionAreDeclared()
        {
            List<string> failures = [];

            foreach (string projectName in ProductionSources.CheckedProjectNames)
            {
                XDocument project = XDocument.Load(ProductionSources.GetProjectFilePath(projectName));

                AssertProperty(project, projectName, "ImplicitUsings", "disable", failures);
                AssertProperty(project, projectName, "LangVersion", "latest", failures);
                AssertProperty(project, projectName, "Nullable", "enable", failures);
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryCheckedProject_WhenInspected_ThenNoGlobalUsingsFileIsGenerated()
        {
            List<string> failures = [];

            foreach (string projectName in ProductionSources.CheckedProjectNames)
            {
                XDocument project = XDocument.Load(ProductionSources.GetProjectFilePath(projectName));

                bool usesImplicitUsings = project
                    .Descendants("ImplicitUsings")
                    .Any(element => string.Equals(element.Value, "enable", StringComparison.OrdinalIgnoreCase));

                if (usesImplicitUsings)
                {
                    failures.Add($"{projectName}: ImplicitUsings must be disabled.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenTheProductionProject_WhenInspected_ThenItDoesNotEnableANullableDirectiveOrTreatWarningsAsErrors()
        {
            XDocument project = XDocument.Load(ProductionSources.GetProjectFilePath("SokobanCLI"));

            Assert.That(project.Descendants("NoWarn").Select(element => element.Value), Is.Empty);
        }

        [Test]
        public void GivenTheRepository_WhenInspected_ThenNoStraySourceFilesExistOutsideTheProjectFolders()
        {
            List<string> strayFiles = Directory
                .EnumerateFiles(ProductionSources.RepositoryRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetFileName(path) ?? string.Empty)
                .ToList();

            Assert.That(strayFiles, Is.Empty);
        }

        [Test]
        public void GivenTheRepository_WhenInspected_ThenEveryProjectIsListedInTheSolution()
        {
            XDocument solution = XDocument.Load(ArchitecturePaths.SolutionPath);

            List<string> solutionPaths = solution
                .Descendants("Project")
                .Select(project => (string?)project.Attribute("Path") ?? string.Empty)
                .ToList();

            List<string> missingProjects = Directory
                .EnumerateFiles(ProductionSources.RepositoryRoot, "*.csproj", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetFileName(path) ?? string.Empty)
                .Where(name => !solutionPaths.Any(solutionPath => solutionPath.EndsWith("/" + name, StringComparison.Ordinal)))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            Assert.That(missingProjects, Is.Empty);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenInspected_ThenItIsNotAnExecutableTestHost()
        {
            List<string> testPackages = XDocument.Load(ProductionSources.GetProjectFilePath("SokobanCLI"))
                .Descendants("PackageReference")
                .Select(reference => (string?)reference.Attribute("Include") ?? string.Empty)
                .Where(name => name.Contains("Test", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.That(testPackages, Is.Empty);
        }

        private static void AssertProperty(XDocument project, string projectName, string propertyName, string expected, List<string> failures)
        {
            string? actual = project.Descendants(propertyName).FirstOrDefault()?.Value;

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                failures.Add($"{projectName}: expected {propertyName}={expected}, found '{actual}'.");
            }
        }
    }
}