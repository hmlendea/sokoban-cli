using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using SokobanCLI.Models;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using ReflectionAssembly = System.Reflection.Assembly;

namespace SokobanCLI.ArchitectureTests
{
    /// <summary>
    /// Enforces the documented layer dependency matrix:
    /// Models must not depend on game logic, input, UI, or rendering;
    /// Graphics must not depend on models, game logic, input, or UI;
    /// Input must not depend on models, game logic, graphics, or UI;
    /// GameLogic must not depend on input or UI;
    /// no type may depend on the composition root.
    /// </summary>
    [TestFixture]
    public sealed class ProductionArchitectureTests
    {
        private static readonly ReflectionAssembly ProductionAssembly = typeof(World).Assembly;

        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(ProductionAssembly)
            .Build();

        private static readonly IObjectProvider<IType> ModelTypes = Types()
            .That()
            .ResideInNamespace("SokobanCLI.Models");

        private static readonly IObjectProvider<IType> GameLogicTypes = Types()
            .That()
            .ResideInNamespaceMatching("SokobanCLI\\.GameLogic.*");

        private static readonly IObjectProvider<IType> GraphicsTypes = Types()
            .That()
            .ResideInNamespaceMatching("SokobanCLI\\.Graphics.*");

        private static readonly IObjectProvider<IType> RenderingTypes = Types()
            .That()
            .ResideInNamespace("SokobanCLI.Graphics");

        private static readonly IObjectProvider<IType> InputTypes = Types()
            .That()
            .ResideInNamespaceMatching("SokobanCLI\\.Input.*");

        private static readonly IObjectProvider<IType> UiTypes = Types()
            .That()
            .ResideInNamespaceMatching("SokobanCLI\\.Ui.*");

        private static readonly IObjectProvider<IType> CompositionRootTypes = Types()
            .That()
            .HaveFullName("SokobanCLI.Program");

        private static readonly string[] ForbiddenAssemblyFragments =
        [
            "NUnit",
            "Moq",
            "Tests",
            "xunit",
            "EntityFramework",
            "Dapper",
            "SqlClient",
            "Npgsql",
            "Dynamo"
        ];

        [Test]
        public void GivenTheProductionAssembly_WhenLoaded_ThenItIsSokobanCLI()
            => Assert.That(ProductionAssembly.GetName().Name, Is.EqualTo("SokobanCLI"));

        [Test]
        public void GivenTheProductionAssembly_WhenLoaded_ThenEveryBoundarySelectsTypes()
        {
            Assert.That(ModelTypes.GetObjects(Architecture), Is.Not.Empty, "No model types were selected.");
            Assert.That(GameLogicTypes.GetObjects(Architecture), Is.Not.Empty, "No game-logic types were selected.");
            Assert.That(GraphicsTypes.GetObjects(Architecture), Is.Not.Empty, "No graphics types were selected.");
            Assert.That(InputTypes.GetObjects(Architecture), Is.Not.Empty, "No input types were selected.");
            Assert.That(UiTypes.GetObjects(Architecture), Is.Not.Empty, "No UI types were selected.");
            Assert.That(CompositionRootTypes.GetObjects(Architecture), Has.Exactly(1).Items, "The composition root was not found.");
        }

        [Test]
        public void GivenModelTypes_WhenChecked_ThenTheyDependOnNoApplicationBoundary()
        {
            Assert.That(ModelTypes.GetObjects(Architecture), Is.Not.Empty);

            IArchRule rule = Types()
                .That()
                .Are(ModelTypes)
                .Should()
                .NotDependOnAny(GameLogicTypes)
                .AndShould()
                .NotDependOnAny(InputTypes)
                .AndShould()
                .NotDependOnAny(UiTypes)
                .AndShould()
                .NotDependOnAny(RenderingTypes)
                .As("models must not depend on game logic, input, UI, or rendering");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenGraphicsTypes_WhenChecked_ThenTheyDoNotDependOnHigherBoundaries()
        {
            Assert.That(GraphicsTypes.GetObjects(Architecture), Is.Not.Empty);

            IArchRule rule = Types()
                .That()
                .Are(GraphicsTypes)
                .Should()
                .NotDependOnAny(ModelTypes)
                .AndShould()
                .NotDependOnAny(GameLogicTypes)
                .AndShould()
                .NotDependOnAny(InputTypes)
                .AndShould()
                .NotDependOnAny(UiTypes)
                .As("graphics must not depend on models, game logic, input, or UI");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenInputTypes_WhenChecked_ThenTheyDoNotDependOnOtherBoundaries()
        {
            Assert.That(InputTypes.GetObjects(Architecture), Is.Not.Empty);

            IArchRule rule = Types()
                .That()
                .Are(InputTypes)
                .Should()
                .NotDependOnAny(ModelTypes)
                .AndShould()
                .NotDependOnAny(GameLogicTypes)
                .AndShould()
                .NotDependOnAny(GraphicsTypes)
                .AndShould()
                .NotDependOnAny(UiTypes)
                .As("input must not depend on models, game logic, graphics, or UI");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenGameLogicTypes_WhenChecked_ThenTheyDoNotDependOnInputOrUi()
        {
            Assert.That(GameLogicTypes.GetObjects(Architecture), Is.Not.Empty);

            IArchRule rule = Types()
                .That()
                .Are(GameLogicTypes)
                .Should()
                .NotDependOnAny(InputTypes)
                .AndShould()
                .NotDependOnAny(UiTypes)
                .As("game logic must not depend on input or UI");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenApplicationTypes_WhenChecked_ThenTheyDoNotDependOnTheCompositionRoot()
        {
            Assert.That(CompositionRootTypes.GetObjects(Architecture), Is.Not.Empty);

            IArchRule rule = Types()
                .That()
                .AreNot(CompositionRootTypes)
                .Should()
                .NotDependOnAny(CompositionRootTypes)
                .As("only the composition root may drive the application");

            rule.Check(Architecture);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenChecked_ThenItDeclaresNoInternalsVisibleTo()
        {
            List<string> exposedAssemblies = ProductionAssembly
                .GetCustomAttributes<InternalsVisibleToAttribute>()
                .Select(attribute => attribute.AssemblyName)
                .ToList();

            Assert.That(exposedAssemblies, Is.Empty);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenChecked_ThenItContainsNoTestTypes()
        {
            List<System.Type> testTypes = ProductionAssembly
                .GetTypes()
                .Where(type => type.Namespace is not null &&
                    type.Namespace.Contains("Tests", StringComparison.Ordinal))
                .ToList();

            Assert.That(testTypes, Is.Empty);
        }

        [Test]
        public void GivenTheProductionAssembly_WhenChecked_ThenItReferencesNoTestOrDatabaseLibrary()
        {
            List<string> forbiddenReferences = ProductionAssembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .Where(name => ForbiddenAssemblyFragments.Any(fragment =>
                    name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Assert.That(forbiddenReferences, Is.Empty);
        }
    }
}