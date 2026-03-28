using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

internal class VariableAssignmentPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "VariableAssignment"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        int? anchorIndex = null;
        do
        {
            if (Walker.CurrentText.EqualsAnyOf("{", ";"))
                anchorIndex = Walker.CurrentIndex;

            if (!Walker.CurrentText.EqualsAnyOf("="))
                continue;

            var assignmentIndex = Walker.CurrentIndex;
            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentText.EqualsAnyOf("new"))
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

            var afterIndex = Walker.CurrentIndex;
            // int a = 5>>;<< anchor index
            // int test >>=<< assignment index
            // if the gap is > 2, then there's type before variable's name 
            var diff = assignmentIndex - (anchorIndex.HasValue ? anchorIndex + 1 : 0);
            if (diff > 2)
            {
                Walker.CurrentIndex = anchorIndex is null ? 0 : anchorIndex.Value + 1;
                var typeWalker = new TypeWalker(Walker, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
                Walker.CurrentIndex = afterIndex;
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}