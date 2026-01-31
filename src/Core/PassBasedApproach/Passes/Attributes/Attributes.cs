using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Attributes;

internal class AttributesPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "Attributes"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        return new PassResult();
    }
}