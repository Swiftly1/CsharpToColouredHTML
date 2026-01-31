using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ObjectInitializer;

internal class ObjectInitializerPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ObjectInitializer"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        return new PassResult();
    }
}