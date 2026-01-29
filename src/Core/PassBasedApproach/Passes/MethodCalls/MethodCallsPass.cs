using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.MethodCalls;

internal class MethodCallsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "MethodCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

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
                var result = IsFunctionCall();
                if (result.Success)
                {
                    Walker.MarkNodeAs(NodeColors.Method);

                    TryMarkChainBackwards(result.NodesWalkedOver);

                    if (!Walker.MoveNext(2))
                        continue;

                    Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
                }
            }

        } while (Walker.MoveNext());

        return new PassResult();
    }

    private void TryMarkChainBackwards(List<NodeInternalRepresentation> nodesToMark)
    {
        var identifiers = nodesToMark
            .Where(x => x.ClassificationType == ClassificationTypeNames.Identifier)
            .ToList();

        if (identifiers.Count == 0)
            return;

        for (int i = 0; i < identifiers.Count; i++)
        {
            var current = identifiers[i];

            var identifiersBeforeCurrent = identifiers.Where((x, index) => index > i).ToList();
            var thereIsVariableBefore = identifiersBeforeCurrent
                .Any(x => Walker.CheckIfLooksLikeVariable(x).IsVariable);

            if (i == identifiers.Count - 1)
            {
                Walker!.MarkNodeAs(current, Walker.ResolveUnkownName(current));
            }
            else
            {
                if (thereIsVariableBefore)
                    Walker!.MarkNodeAs(current, Walker.ResolveVariable(current));
                else
                    Walker!.MarkNodeAs(current, NodeColors.Namespace);
            }
        }
    }

    private (bool Success, List<NodeInternalRepresentation> NodesWalkedOver) IsFunctionCall()
    {
        var offset = 1;

        var list = new List<NodeInternalRepresentation>();

        while (Walker!.TryPeekBehind(out var current, offset))
        {
            offset++;
            list.Add(current);

            if (current.Text.EqualsAnyOf("new"))
                return (false, []);

            // Reject:
            // public IActionResult Index()
            if (current.Text.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
                return (false, []);

            if (current.Text.EqualsAnyOf(";", "}", "{", "=", ",", ")"))
                return (true, list);

            var validClassification = current.ClassificationType.EqualsAnyOf(ValidClassificationsToCheck);
            var isType = current.Text.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray());
            var isGeneric = current.Text.EqualsAnyOf("<", ",", ">");
            var isOperator = current.Text.EqualsAnyOf(".");

            var result = validClassification || isType || isGeneric || isOperator;

            if (!result)
                return (true, []);
        }

        return (true, list);
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