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
    /// Enforces the null-handling, exception, and casting rules of <c>csharp.error-handling.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class ErrorHandlingConventionTests
    {
        private static readonly string[] SensitiveNameFragments =
        [
            "password", "passwd", "secret", "token", "apikey", "apiKey", "credential", "cardnumber", "pan", "privatekey"
        ];

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNullChecksUseIsNullPatterns()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BinaryExpressionSyntax binary in tree.GetRoot().DescendantNodes().OfType<BinaryExpressionSyntax>())
                {
                    if (!binary.IsKind(SyntaxKind.EqualsExpression) &&
                        !binary.IsKind(SyntaxKind.NotEqualsExpression))
                    {
                        continue;
                    }

                    bool comparesNull = binary.Right.IsKind(SyntaxKind.NullLiteralExpression) ||
                        binary.Left.IsKind(SyntaxKind.NullLiteralExpression);

                    if (!comparesNull)
                    {
                        continue;
                    }

                    matched += 1;
                    violations.Add(ProductionSources.Describe(binary, "null comparison must use 'is null' / 'is not null'."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoNullCoalescingOperatorIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BinaryExpressionSyntax binary in tree.GetRoot().DescendantNodes().OfType<BinaryExpressionSyntax>())
                {
                    matched += 1;

                    if (binary.IsKind(SyntaxKind.CoalesceExpression))
                    {
                        violations.Add(ProductionSources.Describe(binary, "'??' is prohibited; '??=' remains permitted."));
                    }
                }
            }

            ProductionSources.AssertClean("No null-coalescing operator", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenRethrowsAreBare()
        {
            List<string> violations = [];
            int matchedThrow = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (CatchClauseSyntax catchClause in tree.GetRoot().DescendantNodes().OfType<CatchClauseSyntax>())
                {
                    foreach (ThrowStatementSyntax throwStatement in catchClause.Block.DescendantNodes().OfType<ThrowStatementSyntax>())
                    {
                        matchedThrow += 1;

                        if (throwStatement.Expression is not null)
                        {
                            violations.Add(ProductionSources.Describe(throwStatement, "rethrow must use a bare 'throw;'."));
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoEmptyCatchBlockExists()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (CatchClauseSyntax catchClause in tree.GetRoot().DescendantNodes().OfType<CatchClauseSyntax>())
                {
                    matched += 1;

                    if (catchClause.Block.Statements.Count == 0)
                    {
                        violations.Add(ProductionSources.Describe(catchClause, "empty catch block."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoCustomExceptionTypeIsDeclaredWithoutNeed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ClassDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (!type.Identifier.ValueText.EndsWith("Exception", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;
                    violations.Add(ProductionSources.Describe(type, $"custom exception '{type.Identifier.ValueText}' should be replaced with a BCL exception."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoExceptionIsConstructedWithASensitiveMessage()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    if (!creation.Type.ToString().EndsWith("Exception", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    matched += 1;

                    foreach (ArgumentSyntax argument in creation.ArgumentList?.Arguments ?? default)
                    {
                        foreach (IdentifierNameSyntax identifier in argument.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                        {
                            if (ContainsSensitiveFragment(identifier.Identifier.ValueText))
                            {
                                violations.Add(ProductionSources.Describe(creation, $"exception message may expose '{identifier.Identifier.ValueText}'."));
                            }
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoUnsafeCastIsUsedWhereAsIsPreferred()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (CastExpressionSyntax cast in tree.GetRoot().DescendantNodes().OfType<CastExpressionSyntax>())
                {
                    matched += 1;
                    string targetType = cast.Type.ToString();

                    if (targetType is "int" or "long" or "short" or "byte" or "char" or "float" or "double" or "decimal" or "bool" ||
                        targetType.EndsWith("2D", StringComparison.Ordinal) ||
                        targetType.EndsWith("EventArgs", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(cast, $"unverified cast to '{targetType}'; prefer 'as'."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        private static bool ContainsSensitiveFragment(string name)
            => SensitiveNameFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}