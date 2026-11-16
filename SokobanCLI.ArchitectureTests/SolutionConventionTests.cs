using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Verifies the SDK-style <c>.slnx</c> structure and project ordering conventions.
    /// </summary>
    [TestFixture]
    public sealed class SolutionConventionTests
    {
        private static readonly string[] ExpectedProjectPaths =
        [
            "SokobanCLI/SokobanCLI.csproj",
            "SokobanCLI.ArchitectureTests/SokobanCLI.ArchitectureTests.csproj",
            "SokobanCLI.UnitTests/SokobanCLI.UnitTests.csproj"
        ];

        [Test]
        public void GivenTheSolution_WhenInspected_ThenItUsesTheSdkStyleRootElement()
        {
            string content = File.ReadAllText(ArchitecturePaths.SolutionPath);

            Assert.That(content, Does.Not.StartWith("<?xml"));
            Assert.That(content.TrimStart(), Does.StartWith("<Solution>"));
            Assert.That(content.TrimEnd(), Does.EndWith("</Solution>"));
        }

        [Test]
        public void GivenTheSolution_WhenParsed_ThenItDeclaresNoVersionAndOnlyPathAttributes()
        {
            XDocument solution = XDocument.Load(ArchitecturePaths.SolutionPath);

            Assert.That(solution.Descendants("Version"), Is.Empty);

            List<XAttribute> extraAttributes = solution
                .Descendants("Project")
                .SelectMany(project => project.Attributes())
                .Where(attribute => !string.Equals(attribute.Name.LocalName, "Path", StringComparison.Ordinal))
                .ToList();

            Assert.That(extraAttributes, Is.Empty);
        }

        [Test]
        public void GivenTheSolution_WhenInspected_ThenMainProjectComesFirstAndTheRestAreAlphabetised()
        {
            XDocument solution = XDocument.Load(ArchitecturePaths.SolutionPath);

            List<string> projectPaths = solution
                .Descendants("Project")
                .Select(project => (string?)project.Attribute("Path") ?? string.Empty)
                .ToList();

            Assert.That(projectPaths, Is.EqualTo(ExpectedProjectPaths));
            Assert.That(projectPaths.First(), Is.EqualTo("SokobanCLI/SokobanCLI.csproj"));
            Assert.That(projectPaths.Skip(1), Is.EqualTo(projectPaths.Skip(1).OrderBy(path => path, StringComparer.Ordinal)));
        }

        [Test]
        public void GivenTheSolution_WhenInspected_ThenEveryProjectExistsInASiblingFolderNamedAfterIt()
        {
            XDocument solution = XDocument.Load(ArchitecturePaths.SolutionPath);

            string solutionDirectory = Path.GetDirectoryName(ArchitecturePaths.SolutionPath) ?? string.Empty;

            foreach (XElement project in solution.Descendants("Project"))
            {
                string relativePath = (string?)project.Attribute("Path") ?? string.Empty;
                string projectName = Path.GetFileNameWithoutExtension(relativePath);
                string expectedDirectory = Path.Combine(solutionDirectory, projectName);

                Assert.That(
                    relativePath,
                    Is.EqualTo(projectName + "/" + projectName + ".csproj"),
                    $"{relativePath} must not nest inside a src-style directory.");

                Assert.That(
                    File.Exists(Path.Combine(solutionDirectory, relativePath)),
                    Is.True,
                    $"{expectedDirectory} project file is missing.");
            }
        }
    }
}