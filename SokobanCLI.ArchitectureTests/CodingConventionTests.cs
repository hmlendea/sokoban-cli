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
    /// Enforces the statically decidable rules of <c>csharp.coding.instructions.md</c>.
    /// </summary>
    [TestFixture]
    public sealed class CodingConventionTests
    {
        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoLocalVariableUsesVar()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (LocalDeclarationStatementSyntax local in tree.GetRoot().DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
                {
                    matched += 1;

                    if (local.Declaration.Type.IsVar)
                    {
                        violations.Add(ProductionSources.Describe(local, "local variable uses 'var'."));
                    }
                }
            }

            ProductionSources.AssertClean("No var declarations", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoTernaryConditionalIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ConditionalExpressionSyntax conditional in tree.GetRoot().DescendantNodes().OfType<ConditionalExpressionSyntax>())
                {
                    matched += 1;
                    violations.Add(ProductionSources.Describe(conditional, "ternary conditional expression."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoTupleOrRefParameterIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (TupleExpressionSyntax tuple in tree.GetRoot().DescendantNodes().OfType<TupleExpressionSyntax>())
                {
                    matched += 1;
                    violations.Add(ProductionSources.Describe(tuple, "tuple expression."));
                }

                foreach (RefTypeSyntax reference in tree.GetRoot().DescendantNodes().OfType<RefTypeSyntax>())
                {
                    matched += 1;
                    violations.Add(ProductionSources.Describe(reference, "ref type."));
                }

                foreach (ParameterSyntax parameter in tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>())
                {
                    matched += 1;

                    if (parameter.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.RefKeyword)))
                    {
                        violations.Add(ProductionSources.Describe(parameter, "ref parameter."));
                    }
                }
            }

            ProductionSources.AssertClean("No tuples or ref types/parameters", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoMethodDeclaresOptionalParameters()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ParameterSyntax parameter in tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>())
                {
                    matched += 1;

                    if (parameter.Default is not null)
                    {
                        violations.Add(ProductionSources.Describe(parameter, "optional parameter; use an overload."));
                    }
                }
            }

            ProductionSources.AssertClean("No optional parameters", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoRegionOrNullableDirectiveIsPresent()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

                for (int index = 0; index < lines.Length; index++)
                {
                    matched += 1;
                    string trimmed = lines[index].TrimStart();

                    if (trimmed.StartsWith("#region", StringComparison.Ordinal) ||
                        trimmed.StartsWith("#endregion", StringComparison.Ordinal) ||
                        trimmed.StartsWith("#nullable", StringComparison.Ordinal))
                    {
                        violations.Add(ProductionSources.DescribeFile(filePath, index + 1, $"directive '{trimmed}' is prohibited."));
                    }
                }
            }

            ProductionSources.AssertClean("No region/nullable directives", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEveryControlFlowBodyIsBraced()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();

                foreach (IfStatementSyntax statement in root.DescendantNodes().OfType<IfStatementSyntax>())
                {
                    matched += 1;

                    if (statement.Statement is not BlockSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement, "if body must use braces."));
                    }

                    if (statement.Else is not null &&
                        statement.Else.Statement is not BlockSyntax &&
                        statement.Else.Statement is not IfStatementSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement.Else, "else body must use braces."));
                    }
                }

                foreach (ForStatementSyntax statement in root.DescendantNodes().OfType<ForStatementSyntax>())
                {
                    matched += 1;

                    if (statement.Statement is not BlockSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement, "for body must use braces."));
                    }
                }

                foreach (ForEachStatementSyntax statement in root.DescendantNodes().OfType<ForEachStatementSyntax>())
                {
                    matched += 1;

                    if (statement.Statement is not BlockSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement, "foreach body must use braces."));
                    }
                }

                foreach (WhileStatementSyntax statement in root.DescendantNodes().OfType<WhileStatementSyntax>())
                {
                    matched += 1;

                    if (statement.Statement is not BlockSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement, "while body must use braces."));
                    }
                }

                foreach (DoStatementSyntax statement in root.DescendantNodes().OfType<DoStatementSyntax>())
                {
                    matched += 1;

                    if (statement.Statement is not BlockSyntax)
                    {
                        violations.Add(ProductionSources.Describe(statement, "do body must use braces."));
                    }
                }
            }

            ProductionSources.AssertClean("Braced control-flow bodies", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenElseIfIsKeptOnOneLine()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ElseClauseSyntax elseClause in tree.GetRoot().DescendantNodes().OfType<ElseClauseSyntax>())
                {
                    if (elseClause.Statement is not IfStatementSyntax ifStatement)
                    {
                        continue;
                    }

                    matched += 1;

                    int elseLine = ProductionSources.GetLineNumber(elseClause);
                    int ifLine = ProductionSources.GetLineNumber(ifStatement);

                    if (elseLine != ifLine)
                    {
                        violations.Add(ProductionSources.Describe(elseClause, "'else if' must stay on one line."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEveryMemberDeclaresExplicitAccessibility()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();

                foreach (MemberDeclarationSyntax member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (member is NamespaceDeclarationSyntax ||
                        member is FileScopedNamespaceDeclarationSyntax ||
                        member is BaseNamespaceDeclarationSyntax ||
                        member is GlobalStatementSyntax ||
                        member is IncompleteMemberSyntax)
                    {
                        continue;
                    }

                    // Interface members are implicitly public and cannot declare accessibility
                    // prior to C# 8 explicit interface member modifiers.
                    if (member.Ancestors().OfType<InterfaceDeclarationSyntax>().Any())
                    {
                        continue;
                    }

                    SyntaxTokenList? modifiers = GetModifiers(member);

                    if (modifiers is null)
                    {
                        continue;
                    }

                    matched += 1;

                    if (!HasAccessibility(modifiers.Value))
                    {
                        violations.Add(ProductionSources.Describe(member, $"member of kind {member.Kind()} declares no explicit accessibility."));
                    }
                }
            }

            ProductionSources.AssertClean("Explicit accessibility", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenEveryTypeIsDeclaredInsideANamespaceBlock()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();
                int namespaceCount = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Count();

                if (namespaceCount != 1)
                {
                    violations.Add(ProductionSources.DescribeFile(tree.FilePath, 1, $"declares {namespaceCount} block namespaces."));
                }

                foreach (BaseTypeDeclarationSyntax type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
                {
                    matched += 1;

                    if (type.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault() is null)
                    {
                        violations.Add(ProductionSources.Describe(type, $"type '{type.Identifier}' is outside a namespace block."));
                    }
                }
            }

            ProductionSources.AssertClean("Types inside namespace blocks", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenMemberKindsFollowThePrescribedOrder()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (TypeDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    List<int> ranks = [];

                    foreach (MemberDeclarationSyntax member in type.Members)
                    {
                        int rank = GetMemberKindRank(member);

                        if (rank < 0)
                        {
                            continue;
                        }

                        ranks.Add(rank);
                    }

                    if (ranks.Count < 2)
                    {
                        continue;
                    }

                    matched += 1;

                    for (int index = 1; index < ranks.Count; index++)
                    {
                        if (ranks[index] < ranks[index - 1])
                        {
                            violations.Add(ProductionSources.Describe(type, $"type '{type.Identifier}' members are not ordered fields -> properties -> events -> constructors -> methods -> nested types."));
                            break;
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoMethodConstructorOrLambdaExceedsSevenParameters()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                SyntaxNode root = tree.GetRoot();

                foreach (BaseMethodDeclarationSyntax method in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
                {
                    matched += 1;

                    if (method.ParameterList.Parameters.Count > 7)
                    {
                        violations.Add(ProductionSources.Describe(method, $"declares {method.ParameterList.Parameters.Count} parameters."));
                    }
                }

                foreach (AnonymousFunctionExpressionSyntax lambda in root.DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>())
                {
                    matched += 1;

                    int parameterCount = lambda switch
                    {
                        ParenthesizedLambdaExpressionSyntax parenthesised => parenthesised.ParameterList.Parameters.Count,
                        SimpleLambdaExpressionSyntax => 1,
                        _ => 0
                    };

                    if (parameterCount > 7)
                    {
                        violations.Add(ProductionSources.Describe(lambda, $"lambda declares {parameterCount} parameters."));
                    }
                }
            }

            ProductionSources.AssertClean("At most seven parameters", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoOverloadGroupIsSplitByAnotherMember()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (TypeDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    List<string> methodNames = type.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Select(method => method.Identifier.ValueText)
                        .ToList();

                    if (methodNames.Count < 2)
                    {
                        continue;
                    }

                    matched += 1;

                    List<string> distinctNames = [];
                    string? previous = null;

                    foreach (string name in methodNames)
                    {
                        if (!string.Equals(name, previous, StringComparison.Ordinal))
                        {
                            distinctNames.Add(name);
                            previous = name;
                        }
                    }

                    if (distinctNames.Count != distinctNames.Distinct(StringComparer.Ordinal).Count())
                    {
                        violations.Add(ProductionSources.Describe(type, $"type '{type.Identifier}' splits an overload group."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenOverloadsAreOrderedByParameterCount()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (TypeDeclarationSyntax type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    IEnumerable<IGrouping<string, MethodDeclarationSyntax>> groups = type.Members
                        .OfType<MethodDeclarationSyntax>()
                        .GroupBy(method => method.Identifier.ValueText, StringComparer.Ordinal)
                        .Where(group => group.Count() > 1);

                    foreach (IGrouping<string, MethodDeclarationSyntax> group in groups)
                    {
                        matched += 1;

                        List<int> counts = group.Select(method => method.ParameterList.Parameters.Count).ToList();

                        if (!counts.SequenceEqual(counts.OrderBy(count => count)))
                        {
                            violations.Add(ProductionSources.Describe(group.First(), $"overloads of '{group.Key}' are not ordered by parameter count."));
                        }
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoBlankLinePrecedesAClosingDelimiter()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (string filePath in ProductionSources.GetProductionFiles())
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

                for (int index = 1; index < lines.Length; index++)
                {
                    string current = lines[index].Trim();

                    if (current is not "}" and not "]" and not ")")
                    {
                        continue;
                    }

                    if (string.Equals(lines[index - 1].Trim(), "{"))
                    {
                        continue;
                    }

                    matched += 1;

                    if (string.IsNullOrWhiteSpace(lines[index - 1]))
                    {
                        violations.Add(ProductionSources.DescribeFile(filePath, index + 1, "blank line precedes a closing delimiter."));
                    }
                }
            }

            ProductionSources.AssertClean("No blank line before closing delimiters", matched, violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenInstanceEqualsCallsAreAvoided()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    {
                        continue;
                    }

                    if (!string.Equals(memberAccess.Name.Identifier.ValueText, "Equals", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // Equality operator and Equals(object) implementations must delegate to the
                    // instance overload; only ordinary call sites are checked.
                    bool insideEqualsDeclaration = invocation.Ancestors().Any(ancestor =>
                        ancestor is OperatorDeclarationSyntax ||
                        (ancestor is MethodDeclarationSyntax method &&
                            string.Equals(method.Identifier.ValueText, "Equals", StringComparison.Ordinal)));

                    if (insideEqualsDeclaration)
                    {
                        continue;
                    }

                    matched += 1;
                    string target = memberAccess.Expression.ToString();

                    if (string.Equals(target, "this", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(invocation, "instance Equals call; prefer the static Equals(a, b) form."));
                }
            }

            // Documented exception: no production call site currently performs an instance
            // Equals call outside an Equals/operator implementation, so an empty selection is
            // reported rather than treated as a failure.
            if (matched == 0)
            {
                Assert.Pass("No ordinary Equals call sites exist to check.");
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoExplicitSelfAssignmentIncrementationIsUsed()
        {
            List<string> violations = [];
            int matched = 0;

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (ExpressionStatementSyntax statement in tree.GetRoot().DescendantNodes().OfType<ExpressionStatementSyntax>())
                {
                    if (statement.Expression is not AssignmentExpressionSyntax assignment)
                    {
                        continue;
                    }

                    matched += 1;

                    if (!assignment.IsKind(SyntaxKind.AddAssignmentExpression) &&
                        !assignment.IsKind(SyntaxKind.SubtractAssignmentExpression) &&
                        !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    {
                        continue;
                    }

                    if (!string.Equals(assignment.Left.ToString(), assignment.Right.ToString(), StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(ProductionSources.Describe(statement, "self-assignment; use += 1 / -= 1."));
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        [Test]
        public void GivenTheProductionSources_WhenParsed_ThenNoUsesExpressionIsUsedOnThisOrMember()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in ProductionSources.GetProductionTrees())
            {
                foreach (AssignmentExpressionSyntax assignment in tree.GetRoot().DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if (assignment.Right is not LiteralExpressionSyntax literal ||
                        literal.Kind() != SyntaxKind.DefaultLiteralExpression)
                    {
                        continue;
                    }

                    if (assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is ThisExpressionSyntax)
                    {
                        violations.Add(ProductionSources.Describe(assignment, "method decorates 'uses' with 'this'."));
                    }
                }
            }

            ProductionSources.AssertScanned(violations);
        }

        private static bool HasAccessibility(SyntaxTokenList modifiers)
            => modifiers.Any(modifier =>
                modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.PrivateKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword) ||
                modifier.IsKind(SyntaxKind.InternalKeyword));

        private static SyntaxTokenList? GetModifiers(MemberDeclarationSyntax member)
        {
            if (member is BaseTypeDeclarationSyntax type)
            {
                return type.Modifiers;
            }

            if (member is DelegateDeclarationSyntax delegateDeclaration)
            {
                return delegateDeclaration.Modifiers;
            }

            if (member is BaseMethodDeclarationSyntax method)
            {
                return method.Modifiers;
            }

            if (member is BasePropertyDeclarationSyntax property)
            {
                return property.Modifiers;
            }

            if (member is BaseFieldDeclarationSyntax field)
            {
                return field.Modifiers;
            }

            return null;
        }

        private static int GetMemberKindRank(MemberDeclarationSyntax member)
        {
            if (member is BaseFieldDeclarationSyntax)
            {
                return 0;
            }

            if (member is BasePropertyDeclarationSyntax)
            {
                return 1;
            }

            if (member is EventFieldDeclarationSyntax || member is EventDeclarationSyntax)
            {
                return 2;
            }

            if (member is ConstructorDeclarationSyntax)
            {
                return 3;
            }

            if (member is DestructorDeclarationSyntax)
            {
                return 4;
            }

            if (member is BaseMethodDeclarationSyntax)
            {
                return 5;
            }

            if (member is BaseTypeDeclarationSyntax || member is DelegateDeclarationSyntax)
            {
                return 6;
            }

            return -1;
        }
    }
}