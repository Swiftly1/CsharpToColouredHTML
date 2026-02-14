using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.CasualExpressions;

internal class CasualExpressionsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "CasualExpressions"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Logger.Info($" - {nameof(StandaloneFunctionCalls)}");
        StandaloneFunctionCalls(input);

        Logger.Info($" - {nameof(ExpressionsInTheMiddleOfOtherExpression)}");
        ExpressionsInTheMiddleOfOtherExpression(input);

        Logger.Info($" - {nameof(OperatorBeforeExpression)}");
        OperatorBeforeExpression(input);
        return new PassResult();
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

            if (Walker.TryPeekBehind(out var dot))
            {
                if (dot.IsChain)
                    continue;

                if (dot.Text != ".")
                    continue;
            }

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.FromTheMiddle);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.FromTheMiddle);
            }
        } while (Walker.MoveNext());
    }

    private void StandaloneFunctionCalls(List<Node> input)
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

                if (!semicolon.Text.EqualsAnyOf(";", "{", "}"))
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