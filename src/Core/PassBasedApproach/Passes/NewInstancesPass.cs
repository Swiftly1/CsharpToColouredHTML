using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class NewInstancesPass : Pass
{
    public override string Name { get => "NewInstances"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public NewInstancesPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (Walker.CurrentText != "new")
                continue;

            Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Keyword, true);

            if (!Walker.MoveNext())
                continue;

            var isChain = Walker.CurrentNode.IsChain;
            var justType = isChain ?
                new NodeEnumerationHelper(Walker.CurrentNode.Nodes) :
                Walker;

            var typeWalker = new TypeWalker(justType, Context);
            if (typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType))
            {
                TrySaveMetadata();
            }

        } while (Walker.MoveNext());

        return new PassResult();
    }

    private void TrySaveMetadata()
    {
        if (!Walker.TryPeekAhead(out var bracket) || bracket.Text != "{")
            return;

        var startIndex = Walker.CurrentIndex + 1;
        var offset = 2;
        var bracketsCounter = 1;

        while (Walker.TryPeekAhead(out var current, offset))
        {
            if (current.Text == ";")
            {
                var endIndex = Walker.CurrentIndex + offset;
                Context.FoundObjectInitializersRanges.Add((startIndex, endIndex));
            }

            if (current.Text == "{")
            {
                bracketsCounter++;
            }

            if (current.Text == "}")
            {
                bracketsCounter--;

                if (bracketsCounter <= 0)
                {
                    var endIndex = Walker.CurrentIndex + offset;
                    Context.FoundObjectInitializersRanges.Add((startIndex, endIndex));
                }
            }

            if (bracketsCounter <= 0)
                return;

            offset++;
        }
    }
}