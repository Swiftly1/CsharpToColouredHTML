using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PrintNodes;

internal class PrintNodesPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "PrintNodes"; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        foreach (var node in input)
        {
            if (node.IsChain)
            {
                Logger.Info("[", 1);
                foreach (var c in node.Nodes)
                {
                    Logger.Info(c.ToString(), 2);
                }
                Logger.Info("]", 1);
            }
            else
            {
                Logger.Info(node.Node.ToString());
            }
        }

        return new PassResult();
    }
}