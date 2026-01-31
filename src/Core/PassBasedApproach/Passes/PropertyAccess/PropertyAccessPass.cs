using CsharpToColouredHTML.Core.Miscs;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PropertyAccess;

internal class PropertyAccessPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "PropertyAccess"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        return new PassResult();
    }
}