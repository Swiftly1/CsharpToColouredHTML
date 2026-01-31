using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class NamespacesPass : Pass
{
    public override string Name { get => "Namespaces"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public NamespacesPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        return new PassResult();
    }
}