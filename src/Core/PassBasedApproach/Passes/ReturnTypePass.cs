using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ReturnType;

internal class ReturnTypePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ReturnType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CC != ClassificationTypeNames.ControlKeyword)
                continue;

            if (Walker.CurrentText != "return")
                continue;

            if (Walker.MoveNext())
            {
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
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}