using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ForEach;

internal class ForeachPass : Pass
{
    public override string Name { get => "Foreach"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public ForeachPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CC != ClassificationTypeNames.ControlKeyword)
                continue;

            if (Walker.CurrentText != "foreach")
                continue;

            if (Walker.MoveNext(2))
            {
                if (Walker.CurrentNode.Text == "var")
                    continue;

                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);

                if (!Walker.MoveNext())
                    continue;

                if (!Walker.MoveNext() || Walker.CurrentNode.Text != "in")
                    continue;

                if (!Walker.MoveNext())
                    continue;

                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}