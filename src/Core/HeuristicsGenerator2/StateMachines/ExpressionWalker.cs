using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private (bool Success, List<Node> Nodes) CollectIfExpression()
    {
        if (CurrentText != "(")
            return (false, new());

        var current = CurrentNode;
        var offset = 0;
        var collectedNodes = new List<Node>();

        var parenthesisCounter = 0;

        do
        {
            collectedNodes.Add(current);

            if (current.Text == "(")
            {
                parenthesisCounter++;
            }
            else if (current.Text == ")")
            {
                parenthesisCounter--;

                if (parenthesisCounter <= 0)
                    return (true, collectedNodes);
            }

            offset++;
        } while (TryPeekAhead(out current, offset));

        return (false, new());
    }

    private (bool Success, List<Node> Nodes) CollectAssignmentExpression(params string[] separators)
    {
        var current = CurrentNode;
        var offset = 0;
        var collectedNodes = new List<Node>();

        do
        {
            collectedNodes.Add(current);

            if (current.Text.EqualsAnyOf(separators))
            {
                return (true, collectedNodes);
            }

            if (!current.ClassificationType.EqualsAnyOf(PropertyChainValidIdentifiers))
            {
                if (current.Text != ".")
                {
                    return (false, collectedNodes);
                }
            }

            offset++;
        } while (TryPeekAhead(out current, offset));

        return (false, new());
    }

    private bool TryWalkBeforeAssignmentExpression()
    {
        var (success, nodes) = CollectAssignmentExpression("=", ";");

        if (!success)
            return false;

        var identifiers = nodes
        .Where(x => x.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
        .ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            Node? entry = nodes[i];
            if (entry.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                MarkNodeAs(entry, NodeColors.Punctuation);
            }
            else if (entry.ClassificationType == ClassificationTypeNames.Operator)
            {
                MarkNodeAs(entry, NodeColors.Operator);
            }
            else if (entry.Id == identifiers[^1].Id)
            {
                if (i < nodes.Count - 2)
                {
                    var next = nodes[i + 1];
                    if (next.Text == "(")
                    {
                        MarkNodeAs(entry, NodeColors.Method);
                        continue;
                    }
                }

                var nameResult = ResolveName(entry);
                if (nameResult.Success)
                {
                    MarkNodeAs(entry, nameResult.Value);
                }
                else
                {
                    MarkNodeAs(entry, NodeColors.PropertyName);
                }
            }
            else if (entry.Id == identifiers[^2].Id)
            {
                var nameResult = ResolveName(entry);
                if (nameResult.Success)
                {
                    MarkNodeAs(entry, nameResult.Value);
                }
                else
                {
                    if (!entry.Text.FirstCharIsUpper())
                    {
                        MarkNodeAs(entry, NodeColors.PropertyName);
                    }
                    else
                    {
                        MarkNodeAs(entry, ResolveClassOrStructName(entry));
                    }
                }
            }
            else
            {
                MarkNodeAs(entry, NodeColors.Namespace);
            }
        }

        _CurrentIndex = _OriginalNodes.IndexOf(nodes.Last());

        return true;
    }


    public bool TryWalkAssignmentExpression()
    {
        if (CurrentText != "=")
            return false;

        MarkNodeAs(NodeColors.Operator);

        if (!MoveNext() || CurrentText == "new")
        {
            MoveBehind();
            return true;
        }

        var (success, nodes) = CollectAssignmentExpression(";");

        if (!success)
        {
            MoveBehind();
            return true;
        }

        var identifiers = nodes
        .Where(x => x.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
        .ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            Node? entry = nodes[i];
            if (entry.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                MarkNodeAs(entry, NodeColors.Punctuation);
            }
            else if (entry.ClassificationType == ClassificationTypeNames.Operator)
            {
                MarkNodeAs(entry, NodeColors.Operator);
            }
            else if (entry.Id == identifiers[^1].Id)
            {
                if (i < nodes.Count - 2)
                {
                    var next = nodes[i + 1];
                    if (next.Text == "(")
                    {
                        MarkNodeAs(entry, NodeColors.Method);
                        continue;
                    }
                }

                var nameResult = ResolveName(entry);
                if (nameResult.Success)
                {
                    MarkNodeAs(entry, nameResult.Value);
                }
                else
                {
                    MarkNodeAs(entry, NodeColors.PropertyName);
                }
            }
            else if (entry.Id == identifiers[^2].Id)
            {
                var nameResult = ResolveName(entry);
                if (nameResult.Success)
                {
                    MarkNodeAs(entry, nameResult.Value);
                }
                else
                {
                    if (!entry.Text.FirstCharIsUpper())
                    {
                        MarkNodeAs(entry, NodeColors.PropertyName);
                    }
                    else
                    {
                        MarkNodeAs(entry, ResolveClassOrStructName(entry));
                    }
                }
            }
            else
            {
                MarkNodeAs(entry, NodeColors.Namespace);
            }
        }

        _CurrentIndex = _OriginalNodes.IndexOf(nodes.Last());

        return true;
    }

    public bool TryWalkIf()
    {
        var collectResult = CollectIfExpression();

        if (!collectResult.Success)
            return false;

        var nodes = collectResult.Nodes;
        var split = SplitGenericNodes(nodes);

        foreach (var group in split)
        {
            var identifiers = group
            .Where(x => x.ClassificationType.EqualsAnyOf(MethodChainValidIdentifiers))
            .ToList();

            for (int i = 0; i < group.Count; i++)
            {
                Node? entry = group[i];
                if (entry.ClassificationType == ClassificationTypeNames.Punctuation)
                {
                    MarkNodeAs(entry, NodeColors.Punctuation);
                }
                else if (entry.ClassificationType == ClassificationTypeNames.Operator)
                {
                    MarkNodeAs(entry, NodeColors.Operator);
                }
                else if (identifiers.Count > 0 && entry.Id == identifiers[^1].Id)
                {
                    if (i < group.Count - 1)
                    {
                        var next = group[i + 1];
                        if (next.Text == "(")
                        {
                            MarkNodeAs(entry, NodeColors.Method);
                            continue;
                        }
                    }

                    var nameResult = ResolveName(entry);
                    if (nameResult.Success)
                    {
                        MarkNodeAs(entry, nameResult.Value);
                    }
                    else
                    {
                        MarkNodeAs(entry, NodeColors.PropertyName);
                    }
                }
                else if (identifiers.Count > 1 && entry.Id == identifiers[^2].Id)
                {
                    var nameResult = ResolveName(entry);
                    if (nameResult.Success)
                    {
                        MarkNodeAs(entry, nameResult.Value);
                    }
                    else
                    {
                        if (!entry.Text.FirstCharIsUpper())
                        {
                            MarkNodeAs(entry, NodeColors.PropertyName);
                        }
                        else
                        {
                            MarkNodeAs(entry, ResolveClassOrStructName(entry));
                        }
                    }
                }
                else
                {
                    if (_SimpleClassificationToColourMapper.TryGetValue(entry.ClassificationType, out var simpleColour))
                    {
                        MarkNodeAs(entry, simpleColour);
                    }
                    else
                    {
                        MarkNodeAs(entry, NodeColors.Namespace);
                    }
                }
            }
        }

        _CurrentIndex = _OriginalNodes.IndexOf(nodes.Last());

        return true;
    }

    private (bool Success, string Value) ResolveName(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.FieldName)
            return (true, NodeColors.FieldName);

        if (node.ClassificationType == ClassificationTypeNames.LocalName)
            return (true, NodeColors.LocalName);

        if (node.ClassificationType == ClassificationTypeNames.PropertyName)
            return (true, NodeColors.PropertyName);

        if (node.ClassificationType == ClassificationTypeNames.ConstantName)
            return (true, NodeColors.ConstantName);

        if (node.ClassificationType == ClassificationTypeNames.ParameterName)
            return (true, NodeColors.ParameterName);

        return (false, string.Empty);
    }

    private List<List<Node>> SplitGenericNodes(List<Node> chainElements)
    {
        var output = new List<List<Node>>();
        var tmp = new List<Node>();

        for (int i = 0; i < chainElements.Count; i++)
        {
            Node? node = chainElements[i];
            tmp.Add(node);

            if (!node.ClassificationType.EqualsAnyOf(_validTypeNameClassifications) && node.Text != "." && node.Text != "?")
            {
                output.Add(tmp);
                tmp = new();
            }
        }

        // just in case
        if (tmp.Count > 0)
            output.Add(tmp);

        return output;
    }

    private string[] PropertyChainValidIdentifiers =
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
        ClassificationTypeNames.PropertyName,
        ClassificationTypeNames.ParameterName
    ];
}
