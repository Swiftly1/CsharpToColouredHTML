using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

internal class VariableAssignmentPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "VariableAssignment"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        var anchors = new string[] { "}", "{", ";" };

        int? anchorIndex = 0;
        do
        {

            if (Walker.CurrentText.EqualsAnyOf("var"))
            {
                if (anchorIndex == null || Walker.CurrentIndex > anchorIndex.Value)
                    anchorIndex = Walker.CurrentIndex;

                continue;
            }

            if (Walker.CurrentText.EqualsAnyOf(anchors))
            {
                if (anchorIndex == null || Walker.CurrentIndex > anchorIndex.Value)
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
                    Walker.CurrentIndex = anchorIndex.Value;

                    if (Walker.CurrentText == ";")
                        Walker.MoveNext();

                    Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
                    Walker.CurrentIndex = assignmentSignIndex;
                }
            }
            else
            {
                Walker.CurrentIndex = anchorIndex.Value;

                if (Walker.CurrentText.EqualsAnyOf(anchors))
                    Walker.MoveNext();
                else
                    throw new Exception("Illegal state");

                Walker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
                Walker.CurrentIndex = assignmentSignIndex;
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}