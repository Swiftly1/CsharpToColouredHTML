using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class VariableAssignmentPass : Pass
{
    public override string Name { get => "VariableAssignment"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public VariableAssignmentPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        int anchorIndex = 0;
        do
        {

            if (Walker.CurrentText.EqualsAnyOf("var"))
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

            if (!Walker.CurrentText.EqualsAnyOf("="))
                continue;

            var assignmentSignIndex = Walker.CurrentIndex;

            if (!Walker.TryPeekBehind(out var localNameCandidate))
                continue;

            if (localNameCandidate.ClassificationType == ClassificationTypeNames.LocalName)
            {
                if (Walker.TryPeekBehind(out var typeOrVar, 2) && typeOrVar.Text != "var")
                {
                    Walker.CurrentIndex = anchorIndex;

                    if (Walker.CurrentText == ";")
                        Walker.MoveNext();

                    Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
                    Walker.CurrentIndex = assignmentSignIndex;
                }
            }
            else
            {
                Walker.CurrentIndex = anchorIndex;

                if (Walker.CurrentText == ";")
                    Walker.MoveNext();

                Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
                Walker.CurrentIndex = assignmentSignIndex;
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

            if (i == identifiers.Count - 1)
            {
                Walker.MarkNodeAs(current, Walker.ResolveClassOrStructName(current));
            }
            else
            {
                Walker.MarkNodeAs(current, NodeColors.Namespace);
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
                return (false, new());

            // Reject:
            // public IActionResult Index()
            if (current.Text.EqualsAnyOf(PassHelpers.CommonKeywordsBeforeTypeName))
                return (false, new());

            if (current.Text.EqualsAnyOf(";", "}", "=", ","))
                return (true, list);

            var validClassification = current.ClassificationType.EqualsAnyOf(ValidClassificationsToCheck);
            var isType = current.Text.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray());
            var isGeneric = current.Text.EqualsAnyOf("<", ",", ">");
            var isOperator = current.Text.EqualsAnyOf(".");

            var result = validClassification || isType || isGeneric || isOperator;

            if (!result)
                return (true, new());
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