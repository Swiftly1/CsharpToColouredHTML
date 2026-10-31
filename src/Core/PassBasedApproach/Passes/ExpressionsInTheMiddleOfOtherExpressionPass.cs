using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ExpressionsInTheMiddleOfOtherExpression;

internal class ExpressionsInTheMiddleOfOtherExpressionPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ExpressionsInTheMiddleOfOtherExpression"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
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

                var counter_behind = 2;
                while (dot.Text.Equals("("))
                {
                    if (Walker.TryPeekBehind(out var behind, counter_behind))
                    {
                        dot = behind;
                        counter_behind++;
                    }
                    else
                    {
                        break;
                    }
                }

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

        return new PassResult();
    }
}