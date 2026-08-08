using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class UsingInstructionPass : Pass
{
    public override string Name { get => "UsingInstruction"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public UsingInstructionPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (Walker.CurrentText != "using")
                continue;

            if (Walker.MoveNext(2))
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