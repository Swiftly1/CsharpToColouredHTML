using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.Miscs;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.EventsRegistration;

internal class EventsRegistrationPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "EventsRegistration"; }

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

            if (Walker.TryPeekBehind(out var op))
            {
                if (op.IsChain)
                    continue;

                if (op.ClassificationType != ClassificationTypeNames.Operator)
                    continue;

                if (!op.Text.EqualsAnyOf("+=", "-="))
                    continue;
            }
            else
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.DelegateRegistrationUnregistration);
            }
            else
            {
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.DelegateRegistrationUnregistration);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}