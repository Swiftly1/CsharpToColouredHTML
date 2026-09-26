using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.ArrayAccess;

internal class ArrayAccessPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ArrayAccess"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        var arrayAccessClassifications = new string[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.MethodName,
        };

        do
        {
            if (Walker.CurrentText != "[")
                continue;

            var isValidPredcesor =
                Walker.TryPeekBehind(out var previous) &&
                previous.ClassificationType.EqualsAnyOf(arrayAccessClassifications);

            if (isValidPredcesor)
            {
                if (!Walker.MoveNext())
                    continue;

                Logger.Info($"ArrayAccess For '{previous.Text}'");
                var expressionWalker = new ExpressionWalker(Walker, Context);
                expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}
