using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class ReturnTypePass : Pass
{
    public override string Name { get => "ReturnType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public ReturnTypePass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.ControlKeyword)
                continue;

            if (Walker.CurrentText != "return")
                continue;

            if (Walker.MoveNext())
            {
                Walker.ConsumeExpressionAhhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}