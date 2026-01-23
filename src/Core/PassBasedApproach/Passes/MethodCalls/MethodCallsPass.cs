using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class MethodCallsPass : Pass
{
    public override string Name { get => "MethodCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public MethodCallsPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            var validClassifications = new string[]
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.MethodName,
            };

            if (!Walker.CC.EqualsAnyOf(validClassifications))
                continue;

            if (Walker.TryPeekAhead(out var parenthesis) && parenthesis.Text == "(")
            {
                var thereIsNewBefore = CheckIfThereIsNewBefore();
                if (!thereIsNewBefore)
                {
                    Walker.MarkNodeAs(NodeColors.Method);
                }
            }

        } while (Walker.MoveNext());

        return new PassResult();
    }

    private bool CheckIfThereIsNewBefore()
    {
        var offset = 1;

        while (Walker!.TryPeekBehind(out var current, offset))
        {
            offset++;

            if (current.Text.EqualsAnyOf("new"))
                return true;

            if (current.Text.EqualsAnyOf(";", "}"))
                return false;

            var validClassification = current.ClassificationType.EqualsAnyOf(ValidClassificationsToCheck);
            var isType = current.Text.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray());
            var isGeneric = current.Text.EqualsAnyOf("<", ",", ">");
            var isOperator = current.Text.EqualsAnyOf(".");

            var result = validClassification || isType || isGeneric || isOperator;

            if (!result)
                return false;
        }

        return false;
    }

    private readonly string[] ValidClassificationsToCheck =
    [
        ClassificationTypeNames.Identifier,
        ClassificationTypeNames.NamespaceName,
        ClassificationTypeNames.ClassName,
        ClassificationTypeNames.StructName,
        ClassificationTypeNames.RecordClassName,
        ClassificationTypeNames.RecordStructName,
        ClassificationTypeNames.InterfaceName,
        ClassificationTypeNames.TypeParameterName
    ];
}