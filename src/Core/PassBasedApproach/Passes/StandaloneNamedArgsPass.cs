using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.StandaloneNamedArgs;

internal class StandaloneNamedArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "StandaloneNamedArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        var isMethod = false;
        do
        {
            if (Walker.CurrentNode.ClassificationType == ClassificationTypeNames.MethodName)
            {
                if (Walker.TryPeekAhead(out var parenthesis) && parenthesis.Text == "(")
                    isMethod = true;
            }

            if (Walker.CurrentNode.Text.EqualsAnyOf(";", "{"))
                isMethod = false;

            if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                continue;

            if (Walker.TryPeekBehind(out var separator))
            {
                if (separator.IsChain)
                    continue;

                if (!separator.Text.EqualsAnyOf(";", "{", "}", "(", ","))
                    continue;
            }

            if (!Walker.TryPeekAhead(out var next) || !next.Text.EqualsAnyOf(":"))
                continue;

            Context.MarkNodeAs(Walker.CurrentNode, isMethod ? NodeColors.ParameterName : NodeColors.PropertyName);

            if (!Walker.MoveNext())
                continue;

            if (!Walker.MoveNext())
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