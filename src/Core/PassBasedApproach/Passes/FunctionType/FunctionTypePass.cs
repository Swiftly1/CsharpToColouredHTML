using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class FunctionTypePass : Pass
{
    public override string Name { get => "FunctionType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public FunctionTypePass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        int anchorIndex = 0;
        do
        {

            if (Walker.CurrentText.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
            {
                if (Walker.CurrentIndex > anchorIndex)
                    anchorIndex = Walker.CurrentIndex;

                continue;
            }

            if (Walker.CurrentText.EqualsAnyOf("}", ";"))
            {
                if (Walker.CurrentIndex > anchorIndex)
                    anchorIndex = Walker.CurrentIndex;

                continue;
            }

            if (!Walker.CC.EqualsAnyOf(ClassificationTypeNames.MethodName, ClassificationTypeNames.PropertyName, ClassificationTypeNames.FieldName))
                continue;

            var methodIndex = Walker.CurrentIndex;

            Walker.CurrentIndex = anchorIndex;
            Walker.MoveNext();
            Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);

            Walker.CurrentIndex = methodIndex;
        } while (Walker.MoveNext());

        return new PassResult();
    }
}