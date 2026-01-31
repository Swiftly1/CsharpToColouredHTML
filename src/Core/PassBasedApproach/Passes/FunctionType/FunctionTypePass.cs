using CsharpToColouredHTML.Core.Miscs;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionType;

internal class FunctionTypePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        return new PassResult();
    }
}