using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.TypeDiscoveryFallback;

internal class TypeDiscoveryFallbackPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "TypeDiscoveryFallback"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Identifier)
                continue;

            Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
        } while (Walker.MoveNext());

        return new PassResult();
    }
}