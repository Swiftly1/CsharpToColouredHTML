using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionArgs;

internal class FunctionArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        foreach (var function in Context.FunctionDeclarationLocations)
        {
        }

        return new PassResult();
    }
}