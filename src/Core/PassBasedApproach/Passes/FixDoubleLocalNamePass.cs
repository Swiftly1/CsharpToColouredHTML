using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

internal class FixDoubleLocalNamePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FixDoubleLocalName"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.TryPeekBehind(out var prev) &&
                Walker.CC == ClassificationTypeNames.LocalName &&
                prev.ClassificationType == ClassificationTypeNames.LocalName)
            {
                if (Context.FoundClasses.Contains(prev.Text))
                {
                    Context.MarkNodeAs(prev, NodeColors.Class, true, true);
                }

                if (Context.FoundStructs.Contains(prev.Text))
                {
                    Context.MarkNodeAs(prev, NodeColors.Struct, true, true);
                }

                if (Context.FoundInterfaces.Contains(prev.Text))
                {
                    Context.MarkNodeAs(prev, NodeColors.Interface, true, true);
                }
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}