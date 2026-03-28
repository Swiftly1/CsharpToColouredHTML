using System.Diagnostics.Metrics;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

internal class GenericsMarkerPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "GenericsMarker"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            if (Walker.CurrentNode.Colour != NodeColors.DefaultColour)
                continue;

            var identifier = Walker.CurrentNode;

            if (!Walker.MoveNext())
                continue;

            if (!Walker.CurrentNode.IsChain &&
                Walker.CurrentText == "<" &&
                Walker.CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                var genericsCounter = 0;
                var found = false;
                do
                {
                    if (Walker.CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
                    {
                        if (Walker.CurrentText == "<") genericsCounter++;
                        if (Walker.CurrentText == ">") genericsCounter--;
                    }

                    if (genericsCounter == 0)
                    {
                        found = true;
                        break;
                    }

                } while (Walker.MoveNext());

                if (found)
                {
                    if (!Walker.MoveNext())
                        continue;

                    var afterGenerics = Walker.CurrentNode;

                    if (afterGenerics.Text == ".")
                    {
                        var colour = Context.NameResolver.ResolveClassOrStructName(identifier);
                        Context.MarkNodeAs(identifier, colour, true);
                    }
                    else if (afterGenerics.Text == "(")
                    {
                        Context.MarkNodeAs(identifier, NodeColors.Method, true);
                    }
                }
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}