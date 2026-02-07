using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionType;

internal class FunctionTypePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CurrentText.EqualsAnyOf(Context.NameResolver.CommonKeywordsBeforeTypeName))
            {
                // get last keyword before method's type
                if (Walker.TryPeekAhead(out var nextKeyword) && !nextKeyword.IsChain &&
                    nextKeyword.Text.EqualsAnyOf(Context.NameResolver.CommonKeywordsBeforeTypeName))
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
                Context.NameResolver.MarkLastElementOfChainAsClassOrStruct(type);

        } while (Walker.MoveNext());
        return new PassResult();
    }
}