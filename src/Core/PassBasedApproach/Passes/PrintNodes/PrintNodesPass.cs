using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PrintNodes;

internal class PrintNodesPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "PrintNodes"; }

    public override PassResult Run(List<Node> input)
    {
        foreach (var node in input)
        {
            if (node.IsChain)
            {
                Logger.Info("[");
                foreach (var c in node.Nodes)
                {
                    Logger.Info(c.ToString(), 1);
                }
                Logger.Info("]");
            }
            else
            {
                Logger.Info(node.ToString());
            }
        }

        return new PassResult();
    }
}