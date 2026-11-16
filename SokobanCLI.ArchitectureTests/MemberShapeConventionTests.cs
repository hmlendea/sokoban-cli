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
    /// Enforces the member-shape rules of <c>csharp.coding.instructions.md</c>:
    /// expression-bodied single statements, static member selection, constant form,
    /// redundant initialisers, and analyser-relevant allocations.
    /// </summary>
    [TestFixture]
    public sealed class MemberShapeConventionTests
    {
        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenSingleStatementMethodsAreExpressionBodied()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    // An expression-bodied method already satisfies the rule.
                    if (method.Body is null && method.ExpressionBody is not null)
                    {
                        matched += 1;
                        continue;
                    }

                    if (method.Body is null || method.Body.Statements.Count != 1)
                    {
                        continue;
                    }

                    StatementSyntax statement = method.Body.Statements[0];

                    if (statement is not ReturnStatementSyntax && statement is not ExpressionStatementSyntax)
                    {
                        continue;
                    }

                    // Operators, conversions, and framework callbacks may legitimately need blocks.
                    if (method.ExplicitInterfaceSpecifier is not null)
                    {
                        continue;
                    }

                    matched += 1;
                    violations.Add(ProductionSources.Describe(method, $"method '{method.Identifier.ValueText}' has a single statement and must be expression-bodied."));
                }
            }

            ProductionSources.AssertClean("Expression-bodied single statements", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenStaticMethodsDoNotAccessInstanceState()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionCompilation.Compilation.SyntaxTrees)
            {
                foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        continue;
                    }

                    matched += 1;

                    bool usesThis = method.DescendantNodes().OfType<ThisExpressionSyntax>().Any() ||
                        method.DescendantNodes().OfType<BaseExpressionSyntax>().Any();

                    if (!usesThis)
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(method, $"static method '{method.Identifier.ValueText}' accesses instance state."));
                }
            }

            ProductionSources.AssertClean("Static methods are instance-free", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoConstantFieldIsDeclaredWithConst()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (FieldDeclarationSyntax field in tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    matched += 1;

                    if (field.Modifiers.Any(SyntaxKind.ConstKeyword))
                    {
                        violations.Add(ProductionSources.Describe(field, "const field; use a 'static [Type] [Name] =>' member instead."));
                    }
                }
            }

            ProductionSources.AssertClean("No const fields", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoRedundantDefaultFieldInitialiserExists()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (FieldDeclarationSyntax field in tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    if (field.Modifiers.Any(SyntaxKind.ConstKeyword))
                    {
                        continue;
                    }

                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        if (variable.Initializer is null)
                        {
                            continue;
                        }

                        matched += 1;

                        if (IsDefaultValue(variable.Initializer.Value, field.Declaration.Type))
                        {
                            violations.Add(ProductionSources.Describe(variable, $"field '{variable.Identifier.ValueText}' repeats the default value."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("No redundant default initialisers", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenRepeatedRegexInstancesAreNotAllocatedInline()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    if (!string.Equals(creation.Type.ToString(), "Regex", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool isStaticField = creation.Ancestors().OfType<FieldDeclarationSyntax>()
                        .Any(field => field.Modifiers.Any(SyntaxKind.StaticKeyword) && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword));

                    if (!isStaticField)
                    {
                        violations.Add(ProductionSources.Describe(creation, "Regex allocated outside a static readonly field."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoMethodOrLambdaTakesAReusableArrayLiteral()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ArgumentSyntax argument in tree.GetRoot().DescendantNodes().OfType<ArgumentSyntax>())
                {
                    if (argument.Expression is ArrayCreationExpressionSyntax ||
                        argument.Expression is CollectionExpressionSyntax)
                    {
                        violations.Add(ProductionSources.Describe(argument, "inline array/collection argument; hoist reusable values into a static readonly field."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoUriSeparatorIsEmbeddedInAString()
        {
            string[] separatorFragments = [ "\"/\"", "'/'" ];

            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (LiteralExpressionSyntax literal in tree.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>())
                {
                    string text = literal.ToString();

                    if (separatorFragments.Any(fragment => string.Equals(text, fragment, StringComparison.Ordinal)))
                    {
                        violations.Add(ProductionSources.Describe(literal, "URI separator is embedded at a call site."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        private static bool IsDefaultValue(ExpressionSyntax value, TypeSyntax type)
            => value switch
            {
                LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression) => true,
                LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression) => true,
                LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NumericLiteralExpression) => IsNumericZero(literal),
                _ => false
            };

        private static bool IsNumericZero(LiteralExpressionSyntax literal)
        {
            string text = literal.Token.ValueText;

            return string.Equals(text, "0", StringComparison.Ordinal) ||
                string.Equals(text, "0.0", StringComparison.Ordinal) ||
                string.Equals(text, "0f", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "0d", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "0m", StringComparison.OrdinalIgnoreCase);
        }
    }
}