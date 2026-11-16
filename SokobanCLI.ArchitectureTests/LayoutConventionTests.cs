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
    /// Enforces the line-splitting and indentation rules of <c>csharp.indentation.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class LayoutConventionTests
    {
        private const int ConditionalLengthThreshold = 72;
        private const int SignatureLengthThreshold = 96;
        private const int ContinuationIndent = 4;

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenLongConditionalsAreSplitOneClausePerLine()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BinaryExpressionSyntax conditional in tree.GetRoot().DescendantNodes().OfType<BinaryExpressionSyntax>())
                {
                    if (!conditional.IsKind(SyntaxKind.LogicalAndExpression) &&
                        !conditional.IsKind(SyntaxKind.LogicalOrExpression))
                    {
                        continue;
                    }

                    if (conditional.Parent is BinaryExpressionSyntax)
                    {
                        continue;
                    }

                    string text = conditional.ToString();
                    string firstLine = text.Split('\n')[0].Trim();

                    if (firstLine.Length <= ConditionalLengthThreshold)
                    {
                        continue;
                    }

                    matched += 1;

                    if (!text.Contains('\n', StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.Describe(conditional, $"conditional line exceeds {ConditionalLengthThreshold} characters and is not split."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenContinuationLinesAreIndentedByFourSpaces()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = File.ReadAllLines(filePath);
                int minimumIndent = int.MaxValue;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    int indent = line.Length - line.TrimStart().Length;
                    minimumIndent = Math.Min(minimumIndent, indent);
                }

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string trimmedLine = line.TrimStart();

                    if (trimmedLine.StartsWith("//", StringComparison.Ordinal) ||
                        trimmedLine.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int indent = line.Length - trimmedLine.Length;
                    int relativeIndent = indent - minimumIndent;

                    if (relativeIndent < 0)
                    {
                        violations.Add(ProductionSources.DescribeFile(filePath, index + 1, "line is indented less than the structural minimum."));
                        continue;
                    }

                    matched += 1;

                    if (relativeIndent % ContinuationIndent != 0)
                    {
                        violations.Add(ProductionSources.DescribeFile(filePath, index + 1, $"indentation of {relativeIndent} is not a multiple of {ContinuationIndent}."));
                    }
                }
            }

            ProductionSources.AssertClean("Four-space indentation", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoLineExceedsTheSignatureThresholdForParameterLists()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (BaseMethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
                {
                    matched += 1;
                    string openingLine = GetOpeningLine(method);

                    if (openingLine.Length <= SignatureLengthThreshold)
                    {
                        continue;
                    }

                    IReadOnlyList<ParameterSyntax> parameters = method.ParameterList.Parameters
                        .OrderBy(parameter => parameter.SpanStart)
                        .ToList();

                    if (parameters.Count < 2)
                    {
                        continue;
                    }

                    bool allOnOwnLines = parameters
                        .Select(parameter => parameter.GetLocation().GetLineSpan().StartLinePosition.Line)
                        .Distinct()
                        .Count() == parameters.Count;

                    if (!allOnOwnLines)
                    {
                        violations.Add(ProductionSources.Describe(method, $"parameter list exceeds {SignatureLengthThreshold} characters and is not split one item per line."));
                    }
                }
            }

            ProductionSources.AssertClean("Split long parameter lists", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenExpressionBodiedMembersPlaceTheArrowOnItsOwnLineWhenLong()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ArrowExpressionClauseSyntax arrowClause in tree.GetRoot().DescendantNodes().OfType<ArrowExpressionClauseSyntax>())
                {
                    matched += 1;
                    SyntaxNode member = arrowClause.Parent!;
                    string openingLine = GetOpeningLine(member);

                    if (openingLine.Length <= SignatureLengthThreshold)
                    {
                        continue;
                    }

                    bool arrowStartsLine = arrowClause
                        .GetLocation()
                        .GetLineSpan()
                        .StartLinePosition
                        .Line != member
                        .GetLocation()
                        .GetLineSpan()
                        .StartLinePosition
                        .Line;

                    if (!arrowStartsLine)
                    {
                        violations.Add(ProductionSources.Describe(member, $"=> must move to its own line when the signature exceeds {SignatureLengthThreshold} characters."));
                    }
                }
            }

            ProductionSources.AssertClean("Long expression-bodied signatures", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEachAutoPropertyOccupiesOneLine()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (PropertyDeclarationSyntax property in tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    if (property.AccessorList is null || property.ExpressionBody is not null)
                    {
                        continue;
                    }

                    bool isAutoProperty = property.AccessorList.Accessors
                        .All(accessor => accessor.Body is null && accessor.ExpressionBody is null);

                    if (!isAutoProperty)
                    {
                        continue;
                    }

                    matched += 1;

                    if (property.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line !=
                        property.AccessorList.GetLocation().GetLineSpan().StartLinePosition.Line ||
                        property.AccessorList.GetLocation().GetLineSpan().StartLinePosition.Line !=
                        property.AccessorList.GetLocation().GetLineSpan().EndLinePosition.Line)
                    {
                        violations.Add(ProductionSources.Describe(property, $"auto-property '{property.Identifier.ValueText}' must occupy one line."));
                    }
                }
            }

            ProductionSources.AssertClean("Single-line auto-properties", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenExactlyOneBlankLineSeparatesMembers()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                string[] lines = File.ReadAllLines(tree.FilePath);

                foreach (TypeDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    for (int index = 1; index < type.Members.Count; index++)
                    {
                        // Consecutive field declarations form a single grouped block and are
                        // permitted to be adjacent; every other adjacent pair of members must
                        // be separated by exactly one blank line.
                        if (type.Members[index] is BaseFieldDeclarationSyntax &&
                            type.Members[index - 1] is BaseFieldDeclarationSyntax)
                        {
                            continue;
                        }

                        matched += 1;

                        // The member's location excludes leading documentation comments and
                        // attributes, so walk upwards over them before counting blank lines.
                        int memberLineIndex = type.Members[index]
                            .GetLocation()
                            .GetLineSpan()
                            .StartLinePosition
                            .Line;

                        int regionTopIndex = memberLineIndex;

                        while (regionTopIndex > 0)
                        {
                            string previousLine = lines[regionTopIndex - 1].TrimStart();

                            if (previousLine.StartsWith("//", StringComparison.Ordinal) ||
                                previousLine.StartsWith("/**", StringComparison.Ordinal) ||
                                previousLine.StartsWith("*", StringComparison.Ordinal) ||
                                previousLine.StartsWith("[", StringComparison.Ordinal))
                            {
                                regionTopIndex -= 1;
                                continue;
                            }

                            break;
                        }

                        int separation = 0;

                        for (int lineIndex = regionTopIndex - 1; lineIndex >= 0; lineIndex--)
                        {
                            if (!string.IsNullOrWhiteSpace(lines[lineIndex]))
                            {
                                break;
                            }

                            separation += 1;
                        }

                        if (separation != 1)
                        {
                            violations.Add(ProductionSources.Describe(type.Members[index], $"member is separated by {separation} blank lines; exactly one is required."));
                        }
                    }
                }
            }

            ProductionSources.AssertClean("One blank line between members", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenModifierOrderIsCanonical()
        {
            string[] expectedOrder =
            [
                "public", "protected", "internal", "private", "file",
                "static", "extern", "new", "virtual", "abstract", "sealed",
                "override", "readonly", "unsafe", "required", "volatile"
            ];

            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (MemberDeclarationSyntax member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    SyntaxTokenList? modifiers = GetModifiers(member);

                    if (modifiers is null || modifiers.Value.Count < 2)
                    {
                        continue;
                    }

                    matched += 1;

                    List<int> ranks = modifiers.Value
                        .Select(modifier => Array.IndexOf(expectedOrder, modifier.ValueText))
                        .Where(rank => rank >= 0)
                        .ToList();

                    if (!ranks.SequenceEqual(ranks.OrderBy(rank => rank)))
                    {
                        violations.Add(ProductionSources.Describe(member, "modifiers are not in canonical order."));
                    }
                }
            }

            ProductionSources.AssertClean("Canonical modifier order", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoConsecutiveBlankLinesExist()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = File.ReadAllLines(filePath);
                int consecutiveBlanks = 0;

                for (int index = 0; index < lines.Length; index++)
                {
                    matched += 1;

                    if (string.IsNullOrWhiteSpace(lines[index]))
                    {
                        consecutiveBlanks += 1;

                        if (consecutiveBlanks > 1)
                        {
                            violations.Add(ProductionSources.DescribeFile(filePath, index + 1, "consecutive blank lines."));
                        }

                        continue;
                    }

                    consecutiveBlanks = 0;
                }
            }

            ProductionSources.AssertClean("No consecutive blank lines", matched, violations);
        }

        private static string GetOpeningLine(SyntaxNode node)
        {
            string text = node.ToString();
            int newLineIndex = text.IndexOf('\n', StringComparison.Ordinal);

            if (newLineIndex < 0)
            {
                return text;
            }

            return text[..newLineIndex].TrimEnd();
        }

        private static SyntaxTokenList? GetModifiers(MemberDeclarationSyntax member)
            => member switch
            {
                BaseTypeDeclarationSyntax type => type.Modifiers,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Modifiers,
                BaseMethodDeclarationSyntax method => method.Modifiers,
                BasePropertyDeclarationSyntax property => property.Modifiers,
                BaseFieldDeclarationSyntax field => field.Modifiers,
                _ => null
            };
    }
}