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

        int? anchorIndex = null;
        do
        {

            if (Walker.CurrentText.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
            {
                if (anchorIndex == null || Walker.CurrentIndex > anchorIndex.Value)
                    anchorIndex = Walker.CurrentIndex;

                continue;
            }

            if (Walker.CurrentText.EqualsAnyOf("}", ";"))
            {
                if (anchorIndex == null || Walker.CurrentIndex > anchorIndex.Value)
                    anchorIndex = Walker.CurrentIndex;

                continue;
            }

            if (!Walker.CC.EqualsAnyOf(ClassificationTypeNames.MethodName, ClassificationTypeNames.PropertyName, ClassificationTypeNames.FieldName))
                continue;

            var funcName = Walker.CurrentText;
            var methodIndex = Walker.CurrentIndex;

            if (!anchorIndex.HasValue)
                continue;

            Walker.CurrentIndex = anchorIndex.Value;
            Walker.MoveNext();
            if (Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType))
            {
                Context.FunctionLocations.Add((funcName, methodIndex));
            }

            Walker.CurrentIndex = methodIndex;
        } while (Walker.MoveNext());

        return new PassResult();
    }
}