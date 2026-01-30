using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ObjectInitializer;

internal class ObjectInitializerPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ObjectInitializer"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Identifier)
                continue;

            var fod = Context
                .FoundObjectInitializersRanges
                .Where(x => x.StartIndex <= Walker.CurrentIndex && x.EndIndex >= Walker.CurrentIndex)
                .ToList();

            if (!fod.Any())
                continue;

            if (fod.Count > 1)
                throw new Exception("TBD, sort by distance between both");

            Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);

        } while (Walker.MoveNext());

        return new PassResult();
    }
}