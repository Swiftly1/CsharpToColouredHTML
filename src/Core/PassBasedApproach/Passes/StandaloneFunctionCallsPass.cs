using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.StandaloneFunctionCalls;

internal class StandaloneFunctionCallsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "StandaloneFunctionCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
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

                if (!separator.Text.EqualsAnyOf(";", "{", "}", ")", "else"))
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

        return new PassResult();
    }
}