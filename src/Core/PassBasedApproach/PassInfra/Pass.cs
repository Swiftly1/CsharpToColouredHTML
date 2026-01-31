using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal abstract class Pass
{
    public abstract string Name { get; }

    public SharedPassContext Context { get; set; }

    public abstract PassResult Run(List<NodeWrapper> input);

    protected Pass(SharedPassContext ctx)
    {
        Context = ctx;
    }
}