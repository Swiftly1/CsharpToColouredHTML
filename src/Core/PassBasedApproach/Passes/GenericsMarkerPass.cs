using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;

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
                var isNew = false;
                var currentNodeIndexInChainedList = NodeFinder.FindNodeInChainedList(Walker.CurrentNode.Id, input);

                if (currentNodeIndexInChainedList > 0)
                {
                    var probablyNew = input[currentNodeIndexInChainedList - 1];
                    if (!probablyNew.IsChain && probablyNew.Text == "new")
                        isNew = true;
                }

                var genericsCounter = 0;
                var found = false;
                var foundNodes = new List<Node>();
                do
                {
                    if (Walker.CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
                    {
                        if (Walker.CurrentText == "<") genericsCounter++;
                        if (Walker.CurrentText == ">") genericsCounter--;
                    }

                    if (genericsCounter >= 1 && Walker.CurrentText != "<")
                        foundNodes.Add(Walker.CurrentNode);

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
                        if (isNew)
                        {
                            var colour = Context.NameResolver.ResolveClassOrStructName(identifier);
                            Context.MarkNodeAs(identifier, colour, true);
                        }
                        else
                        {
                            Context.MarkNodeAs(identifier, NodeColors.Method, true);
                        }
                    }

                    if (foundNodes.Any())
                    {
                        var typeWalker = new TypeWalker(new NodeEnumerationHelper(foundNodes), Context);
                        typeWalker.ConsumeTypeAhead(TypeWalkState.GenericsName, TypeWalkMode.MustBeType);
                    }
                }
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}