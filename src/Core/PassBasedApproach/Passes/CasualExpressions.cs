using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.Miscs;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.CasualExpressions;

internal class CasualExpressionsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "CasualExpressions"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Logger.Info($" - {nameof(EventsRegistration)}");
        EventsRegistration(input);

        Logger.Info($" - {nameof(StandaloneFunctionCalls)}");
        StandaloneFunctionCalls(input);

        Logger.Info($" - {nameof(ExpressionsInTheMiddleOfOtherExpression)}");
        ExpressionsInTheMiddleOfOtherExpression(input);

        Logger.Info($" - {nameof(OperatorBeforeExpression)}");
        OperatorBeforeExpression(input);

        Logger.Info($" - {nameof(FunctionCallExpressions)}");
        FunctionCallExpressions(input);

        Logger.Info($" - {nameof(StandaloneNamedArgs)}");
        StandaloneNamedArgs(input);

        Logger.Info($" - {nameof(StringInterpolation)}");
        StringInterpolation(input);

        Logger.Info($" - {nameof(Fallback)}");
        Fallback(input);
        return new PassResult();
    }


    private void StringInterpolation(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            if (Walker.TryPeekBehind(out var string1, 2))
            {
                if (string1.IsChain)
                    continue;

                if (string1.ClassificationType != ClassificationTypeNames.StringLiteral)
                    continue;
            }

            if (Walker.TryPeekBehind(out var bracket1, 1))
            {
                if (bracket1.IsChain)
                    continue;

                if (bracket1.Text != "{")
                    continue;
            }

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }

    private void OperatorBeforeExpression(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            if (Walker.TryPeekBehind(out var semicolon))
            {
                if (semicolon.IsChain)
                    continue;

                if (semicolon.ClassificationType != ClassificationTypeNames.Operator)
                    continue;
            }
            else
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }

    private void EventsRegistration(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            if (Walker.TryPeekBehind(out var op))
            {
                if (op.IsChain)
                    continue;

                if (op.ClassificationType != ClassificationTypeNames.Operator)
                    continue;

                if (!op.Text.EqualsAnyOf("+=", "-="))
                    continue;
            }
            else
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.DelegateRegistrationUnregistration);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.DelegateRegistrationUnregistration);
            }
        } while (Walker.MoveNext());
    }

    private void ExpressionsInTheMiddleOfOtherExpression(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            var isDot = false;
            var isOperator = false;

            if (Walker.TryPeekBehind(out var dot))
            {
                if (dot.IsChain)
                    continue;

                isDot = dot.Text == ".";
                isOperator = NameResolver.Operators.Contains(dot.Text);

                if (!isDot && !isOperator)
                    continue;
            }
            else
                continue;

            var walkMode = isOperator ? ExpressionWalkMode.Default : ExpressionWalkMode.FromTheMiddle;
            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, walkMode);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, walkMode);
            }
        } while (Walker.MoveNext());
    }

    private void FunctionCallExpressions(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            if (Walker.TryPeekBehind(out var dot))
            {
                if (dot.IsChain)
                    continue;

                if (dot.Text != ",")
                    continue;
            }
            else
                continue;

            if (!Walker.TryPeekAhead(out var next) || next.Text.EqualsAnyOf(":"))
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }

    private void StandaloneFunctionCalls(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                continue;

            if (Walker.TryPeekBehind(out var separator))
            {
                if (separator.IsChain)
                    continue;

                if (!separator.Text.EqualsAnyOf(";", "{", "}", ")"))
                    continue;
            }

            if (!Walker.TryPeekAhead(out var next) || !next.Text.EqualsAnyOf("(", "."))
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }

    private void StandaloneNamedArgs(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        var isMethod = false;
        do
        {
            if (Walker.CurrentNode.ClassificationType == ClassificationTypeNames.MethodName)
            {
                if (Walker.TryPeekAhead(out var parenthesis) && parenthesis.Text == "(")
                        isMethod = true;
            }

            if (Walker.CurrentNode.Text.EqualsAnyOf(";", "{"))
                isMethod = false;

            if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                continue;

            if (Walker.TryPeekBehind(out var separator))
            {
                if (separator.IsChain)
                    continue;

                if (!separator.Text.EqualsAnyOf(";", "{", "}", "(", ","))
                    continue;
            }

            if (!Walker.TryPeekAhead(out var next) || !next.Text.EqualsAnyOf(":"))
                continue;

            Context.MarkNodeAs(Walker.CurrentNode, isMethod ? NodeColors.ParameterName : NodeColors.PropertyName);

            if (!Walker.MoveNext())
                continue;

            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }

    private void Fallback(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                if (Walker.CurrentNode.Nodes[0].Colour != NodeColors.DefaultColour)
                    continue;
            }
            else
            {
                if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                    continue;
            }

            if (Walker.TryPeekBehind(out var semicolon))
            {
                if (semicolon.IsChain)
                    continue;

                if (!semicolon.Text.EqualsAnyOf(";", "{", "}", ")"))
                    continue;
            }

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());
    }
}