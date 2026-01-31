using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Inheritance;

internal class InheritancePass : Pass
{
    public override string Name { get => "Inheritance"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public InheritancePass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (!Walker.CurrentText.EqualsAnyOf("class", "struct"))
                continue;

        } while (Walker.MoveNext());

        return new PassResult();
    }
}