using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.StateMachines;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionArgs;

internal class FunctionArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);


        foreach (var function in Context.FunctionDeclarationLocations)
        {
            var asm = new ArgumentStateMachine(Walker, Context);
            var result = asm.WalkOverArgs(function.Index);
        }

        return new PassResult();
    }
}