using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.Miscs;

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
            // if (TEST)
            // #if (TEST)
            if (!Walker.CC.EqualsAnyOf(ClassificationTypeNames.ControlKeyword, ClassificationTypeNames.PreprocessorKeyword))
                continue;

            if (Walker.CurrentText != "if")
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