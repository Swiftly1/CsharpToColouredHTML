using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionType;

internal class FunctionTypePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CurrentText.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
            {
                // get last keyword before method's type
                if (Walker.TryPeekAhead(out var nextKeyword) && !nextKeyword.IsChain &&
                    nextKeyword.Text.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
                {
                    continue;
                }
            }
            else
            {
                continue;
            }

            var validClassifications = new[]
            {
                ClassificationTypeNames.MethodName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.DelegateName
            };

            if (!(Walker.TryPeekAhead(out var methodName, 2) && methodName.ClassificationType.EqualsAnyOf(validClassifications)))
                continue;

            if (Walker.TryPeekAhead(out var type))
                Walker.MarkLastElementOfChainAsClassOrStruct(type);

        } while (Walker.MoveNext());
        return new PassResult();
    }
}