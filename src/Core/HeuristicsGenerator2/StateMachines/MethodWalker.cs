using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private string[] MethodChainValidIdentifiers =
    [
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.RecordClassName,
            ClassificationTypeNames.RecordStructName,
            ClassificationTypeNames.InterfaceName,
            ClassificationTypeNames.TypeParameterName,
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.MethodName,
            ClassificationTypeNames.ExtensionMethodName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.ParameterName
    ];

    private (bool Success, List<Node> Nodes) CollectChainUntilParenthesis()
    {
        var current = CurrentNode;
        var offset = 0;
        var collectedNodes = new List<Node>();

        do
        {
            if (current.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
            {
                collectedNodes.Add(current);
            }
            else if (current.Text.EqualsAnyOf(".", "?", "?."))
            {
                collectedNodes.Add(current);
            }
            else if (current.Text == "(")
            {
                collectedNodes.Add(current);
                return (collectedNodes.Count > 1, collectedNodes);
            }
            else
            {
                return (false, new List<Node>());
            }

            offset++;
        } while (TryPeekAhead(out current, offset));

        return (false, new List<Node>());
    }

    public bool TryWalkMethod()
    {
        var collectResult = CollectChainUntilParenthesis();

        if (!collectResult.Success)
            return false;

        var nodesReversed = collectResult.Nodes;
        nodesReversed.Reverse();

        var identifiers = nodesReversed
            .Where(x => x.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
            .ToList();

        if (identifiers.Count < 1)
            return false;

        foreach (var entry in nodesReversed)
        {
            if (entry.Id == identifiers[0].Id)
            {
                MarkNodeAs(entry, NodeColors.Method);
            }
            else if (identifiers.Count > 1 && entry.Id == identifiers[1].Id)
            {
                MarkNodeAs(entry, ResolveClassOrStructName(entry));
            }
            else if (entry.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
            {
                var checkResult = IsAlreadyClassOrStruct(entry);

                if (checkResult.Success)
                    MarkNodeAs(entry, checkResult.Value);
                else
                    MarkNodeAs(entry, NodeColors.Namespace);
            }
            else
            {
                MarkNodeAs(entry, NodeColors.Punctuation);
            }
        }

        // last
        _CurrentIndex = _OriginalNodes.IndexOf(nodesReversed.First());
        return true;
    }
}
