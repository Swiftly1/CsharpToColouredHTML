using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class NewInstancesPass : Pass
{
    public override string Name { get => "NewInstances"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public NewInstancesPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (Walker.CurrentText != "new")
                continue;

            if (Walker.MoveNext())
            {
                Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}