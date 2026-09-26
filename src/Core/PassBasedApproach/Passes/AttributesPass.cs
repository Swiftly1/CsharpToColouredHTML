using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Attributes;

internal class AttributesPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "Attributes"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentText != "[")
                continue;

            var canWalkBehind = Walker.TryPeekBehind(out var previous);
            var isValidPredcesor = canWalkBehind && previous.Text.EqualsAnyOf("}", "]");
            if (!canWalkBehind || isValidPredcesor)
            {
                if (!Walker.MoveNext())
                    continue;

                var expressionWalker = new TypeWalker(Walker, Context);
                expressionWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}
