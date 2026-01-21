using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class FunctionsPass : Pass
{
    public override string Name { get => "Functions"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public FunctionsPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override FunctionsResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.MethodName)
                continue;

            var methodIndex = Walker.CurrentIndex;
            bool anyFound = false;

            while (Walker.MoveBehind())
            {
                if (Walker.CurrentText.EqualsAnyOf("}", ";"))
                {
                    break;
                }

                if (Walker.CurrentText.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
                {
                    break;
                }
                anyFound = true;
            }

            if (anyFound)
            {
                Walker.MoveNext();
                Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }

            Walker.CurrentIndex = methodIndex;
        } while (Walker.MoveNext());

        return new FunctionsResult();
    }
}