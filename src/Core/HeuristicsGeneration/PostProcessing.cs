using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private void PostProcess(List<NodeWithDetails> alreadyProcessed)
    {
        // If some identifiers weren't recognized at first attempt, but later instead
        // then we may fix the previous ones.
        var identifiers = alreadyProcessed.Where(x => x.Colour == NodeColors.Identifier).ToList();

        foreach (var entry in identifiers)
        {
            if (entry.SkipIdentifierPostProcessing)
                continue;

            if (IdentifierShouldntBeOverriden(entry, alreadyProcessed))
                continue;

            if (_FoundClasses.Contains(entry.Text))
                entry.Colour = NodeColors.Class;

            if (_FoundInterfaces.Contains(entry.Text))
                entry.Colour = NodeColors.Interface;

            if (_FoundStructs.Contains(entry.Text))
                entry.Colour = NodeColors.Struct;
        }

        for (int i = 0; i < alreadyProcessed.Count; i++)
        {
            var current = alreadyProcessed[i];

            if (current.Colour != NodeColors.Method)
                continue;

            if (i < 4)
                continue;

            var op1 = alreadyProcessed[i - 1];
            var id1 = alreadyProcessed[i - 2];
            var op2 = alreadyProcessed[i - 3];
            var id2 = alreadyProcessed[i - 4];

            if (op1.Colour != NodeColors.Operator || op1.Text != ".")
                continue;

            if (op2.Colour != NodeColors.Operator || op2.Text != ".")
                continue;

            if (id1.Colour == NodeColors.Class && id2.Colour == NodeColors.Class)
            {
                alreadyProcessed[i - 2] = id1 with { Colour = NodeColors.PropertyName };
            }
        }

        for (int i = 0; i < alreadyProcessed.Count; i++)
        {
            var current = alreadyProcessed[i];

            if (current.Colour != NodeColors.Class)
                continue;

            if (ThereIsClassInTheChainBefore(current, alreadyProcessed))
            {
                var types = new List<string>
                {
                    NodeColors.Identifier,
                    NodeColors.PropertyName,
                    NodeColors.FieldName,
                    NodeColors.ConstantName,
                };

                if (i + 1 < alreadyProcessed.Count && !types.Contains(alreadyProcessed[i + 1].Colour))
                {
                    alreadyProcessed[i] = current with { Colour = NodeColors.PropertyName };
                }
                else
                {
                    var chainNames = new List<string>
                    {
                        NodeColors.Class,
                        NodeColors.PropertyName,
                        NodeColors.FieldName,
                        NodeColors.ConstantName,
                        NodeColors.Operator
                    };

                    var nodes = new List<NodeWithDetails>();
                    var tmp = i;
                    while (tmp-- > 0)
                    {
                        var curr = alreadyProcessed[tmp];
                        if (chainNames.Contains(curr.Colour))
                        {
                            if (curr.Colour == NodeColors.Class)
                                alreadyProcessed[tmp] = curr with { Colour = NodeColors.Identifier };
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private bool IdentifierShouldntBeOverriden(NodeWithDetails entry, List<NodeWithDetails> nodes)
    {
        var index = nodes.IndexOf(entry);

        if (index == -1)
            return false;

        var canGoAhead = nodes.CanGoAhead(index);

        if (canGoAhead &&
            nodes[index + 1].ClassificationType == ClassificationTypeNames.Operator &&
            nodes[index + 1].Text.Contains("="))
            return true;

        return false;
    }
}
