using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class IfStatementPass : Pass
{
    public override string Name { get => "IfStatement"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public IfStatementPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CC != ClassificationTypeNames.ControlKeyword)
                continue;

            if (Walker.CurrentText != "if")
                continue;

            if (Walker.MoveNext(2))
            {
                //Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}