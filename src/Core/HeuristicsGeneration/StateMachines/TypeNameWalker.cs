using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private enum TypeWalkStates
    {
        AtIdentifier,
        AtOperator,

        ValueTupleType,
        ValueTupleOperator,
        ValueTupleIdentifier,
        ValueTupleClosing,

        GenericsOpening,
        GenericsClosing,
        GenericsComma
    }

    public bool TypeHasValidIdentifier(Node node)
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.RecordClassName,
            ClassificationTypeNames.RecordStructName,
            ClassificationTypeNames.InterfaceName,
        };

        if (valid_identifiers.Contains(node.ClassificationType))
            return true;

        if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            return _Hints.BuiltInTypes.Contains(node.Text);
        }

        return false;
    }

    internal bool TryConsumeTypeNameAhead()
    {
        return HandleTypeNameAhead(true);
    }

    private bool HandleTypeNameAhead(bool markIt)
    {
        if (!TypeHasValidIdentifier(CurrentNode) && CurrentText != "(")
            return false;

        if (TryPeekBehind(out var nodeBehind) && nodeBehind.Text == ".")
            return false;

        // 0 = currently at Identifier, expecting Operator "."
        // 1 = currently at Operator, expecting Identifier

        var state = TypeWalkStates.AtIdentifier;

        var chainElements = new List<Node> {};
        var offset = 0;

        while (TryPeekAhead(out var peekedNode, offset))
        {
            chainElements.Add(peekedNode);
            switch (state)
            {
                case TypeWalkStates.AtIdentifier:
                {
                        if (TypeHasValidIdentifier(peekedNode))
                        {
                            state = TypeWalkStates.AtOperator;
                        }
                        else if (peekedNode.Text == "(")
                        {
                            state = TypeWalkStates.ValueTupleType;
                        }
                        else
                        {
                            return MarkTypeChain(peekedNode, markIt, chainElements);
                        }

                        break;
                }
                case TypeWalkStates.AtOperator:
                {
                        if (peekedNode.Text == "." || peekedNode.Text == "<" || peekedNode.Text == ",")
                        {
                            state = TypeWalkStates.AtIdentifier;
                        }
                        else if (peekedNode.Text == "(")
                        {
                            return false;
                        }
                        else
                        {
                            return MarkTypeChain(peekedNode, markIt, chainElements);
                        }

                        break;
                }
                case TypeWalkStates.ValueTupleType:
                {
                        if (TypeHasValidIdentifier(peekedNode))
                        {
                            state = TypeWalkStates.ValueTupleOperator;
                        }
                        else if (peekedNode.Text == ")")
                        {
                            state = TypeWalkStates.ValueTupleClosing;
                            goto case TypeWalkStates.ValueTupleClosing;
                        }
                        else if (peekedNode.Text == ",")
                        {
                            state = TypeWalkStates.ValueTupleType;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                }
                case TypeWalkStates.ValueTupleOperator:
                {
                        if (peekedNode.Text == ".")
                        {
                            state = TypeWalkStates.ValueTupleType;
                        }
                        else if (peekedNode.Text.EqualsAnyOf(",", ")"))
                        {
                            state = TypeWalkStates.ValueTupleClosing;
                            goto case TypeWalkStates.ValueTupleClosing;
                        }
                        else if (TypeHasValidIdentifier(peekedNode))
                        {
                            state = TypeWalkStates.ValueTupleIdentifier;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                }
                case TypeWalkStates.ValueTupleIdentifier:
                {
                        if (TypeHasValidIdentifier(peekedNode))
                        {
                            state = TypeWalkStates.ValueTupleClosing;
                        }
                        else if (peekedNode.Text.EqualsAnyOf(")", ","))
                        {
                            state = TypeWalkStates.ValueTupleClosing;
                            goto case TypeWalkStates.ValueTupleClosing;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                }
                case TypeWalkStates.ValueTupleClosing:
                {
                        MarkTupleElements(chainElements, markIt);
                        if (peekedNode.Text == ")")
                        {
                            _CurrentIndex = _OriginalNodes.IndexOf(peekedNode) - 1;
                            return true;
                        }
                        else if (peekedNode.Text == ",")
                        {
                            state = TypeWalkStates.ValueTupleType;
                        }
                        else
                        {
                            _CurrentIndex = _OriginalNodes.IndexOf(peekedNode) - 1;
                            return true;
                        }

                        break;
                }
            }

            offset++;
        }

        return false;
    }

    private bool MarkTypeChain(Node peekedNode, bool markIt, List<Node> chainElements)
    {
        var chainWithoutLastElement = chainElements.SkipLast(1).ToList();

        var allElementsAreValid = chainWithoutLastElement.All(x =>
            TypeHasValidIdentifier(x) ||
            x.ClassificationType == ClassificationTypeNames.Operator || x.Text.EqualsAnyOf(",", "<", ">")
        );

        if (!allElementsAreValid)
            return false;

        var identifiers = chainWithoutLastElement
                          .Where(x => TypeHasValidIdentifier(x))
                          .ToList();

        var operators = chainWithoutLastElement
                          .Where(x => x.ClassificationType == ClassificationTypeNames.Operator || x.Text.EqualsAnyOf(",", "<", ">"))
                          .ToList();

        if (!identifiers.Any())
            return false;

        if (identifiers.Count > 1 && operators.Count != identifiers.Count - 1)
            return false;

        // everything is OK, just do not mark nodes
        if (!markIt)
            return true;

        for (int i = 0; i < chainWithoutLastElement.Count; i++)
        {
            var node = chainWithoutLastElement[i];
            if (node.ClassificationType == ClassificationTypeNames.Operator)
            {
                MarkNodeAs(node, NodeColors.Operator);
            }
            else if (node.Id == identifiers[^1].Id)
            {
                var colour = ResolveName(node);
                MarkNodeAs(node, colour);
            }
            else if (node.Text.EqualsAnyOf("<", ">", ","))
            {
                MarkNodeAs(node, NodeColors.Punctuation);
            }
            else if (i + 1 < chainWithoutLastElement.Count && chainWithoutLastElement[i+1].Text == "<")
            {
                var colour = ResolveName(node);
                MarkNodeAs(node, colour);
            }
            else if (i + 1 < chainWithoutLastElement.Count && chainWithoutLastElement[i+1].Text.EqualsAnyOf(",", ">"))
            {
                var colour = ResolveName(node);
                MarkNodeAs(node, colour);
            }
            else
            {
                MarkNodeAs(node, NodeColors.Namespace);
            }
        }

        _CurrentIndex = _OriginalNodes.IndexOf(peekedNode) - 1;
        return true;
    }

    void MarkTupleElements(List<Node> chainElements, bool markIt)
    {
        if (!markIt)
            return;

        var identifiers = chainElements
                          .Where(x => TypeHasValidIdentifier(x))
                          .ToList();

        var withoutPunctuation = chainElements
                                 .Where(x => x.ClassificationType != ClassificationTypeNames.Punctuation)
                                 .ToList();

        var valueTupleTypeWithName = false;

        if (withoutPunctuation.Count >= 2)
        {
            var last = withoutPunctuation[^1];
            var previous = withoutPunctuation[^2];
            var indexLast = chainElements.IndexOf(last);
            var indexPrev = chainElements.IndexOf(previous);
            if (TypeHasValidIdentifier(last) && TypeHasValidIdentifier(previous) && (indexLast - indexPrev == 1))
                valueTupleTypeWithName = true;
        }

        for (int i = 0; i < chainElements.Count; i++)
        {
            var node = chainElements[i];
            if (node.ClassificationType == ClassificationTypeNames.Operator)
            {
                MarkNodeAs(node, NodeColors.Operator);
            }
            else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                MarkNodeAs(node, NodeColors.Punctuation);
            }
            else if (node.ClassificationType == ClassificationTypeNames.Keyword)
            {
                MarkNodeAs(node, NodeColors.Keyword);
            }
            else if (valueTupleTypeWithName)
            {
                if (node.Id == identifiers[^1].Id)
                {
                    MarkNodeAs(node, NodeColors.Identifier);
                }
                else if (node.Id == identifiers[^2].Id)
                {
                    var color = ResolveName(node);
                    MarkNodeAs(node, color);
                }
                else
                {
                    MarkNodeAs(node, NodeColors.Namespace);
                }
            }
            else if (!valueTupleTypeWithName && node.Id == identifiers[^1].Id)
            {
                var color = ResolveName(node);
                MarkNodeAs(node, color);
            }
            else if (i + 1 < chainElements.Count && chainElements[i+1].Text == "<")
            {
                var color = ResolveName(node);
                MarkNodeAs(node, color);
            }
            else
            {
                MarkNodeAs(node, NodeColors.Namespace);
            }
        }

        chainElements.Clear();
    }

    private bool CheckAndMarkGenericParametersChain()
    {
        if (TryPeekBehind(out var peekedBehind) && peekedBehind.Text == "<")
        {
            var valid_identifiers = new List<string>
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.NamespaceName,
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.StructName,
            };

            // 0 = currently at Identifier, expecting Punctuation "," or "."
            // 1 = currently at Punctuation, expecting Identifier

            var state = 0;
            var indexAhead = 1;

            var chainElements = new List<Node> { CurrentNode };

            while (TryPeekAhead(out var peekedNode, indexAhead))
            {
                chainElements.Add(peekedNode);
                if (state == 0)
                {
                    if (peekedNode.Text.EqualsAnyOf(",", "."))
                    {
                        state = 1;
                        indexAhead++;
                        continue;
                    }
                    else
                    {
                        if (peekedNode.Text == ">")
                        {
                            var chainSplit = SplitGenericNodes(chainElements);

                            foreach (var chain in chainSplit)
                            {
                                var identifiers = chain.Where(TypeHasValidIdentifier).ToList();

                                foreach (var entry in chain)
                                {
                                    if (entry.ClassificationType == ClassificationTypeNames.Punctuation)
                                    {
                                        MarkNodeAs(entry, ClassificationTypeNames.Punctuation);
                                    }
                                    else if (entry.ClassificationType == ClassificationTypeNames.Operator)
                                    {
                                        MarkNodeAs(entry, ClassificationTypeNames.Operator);
                                    }
                                    else if (entry.Id == identifiers[^1].Id)
                                    {
                                        var colour = ResolveName(entry);
                                        MarkNodeAs(entry, colour);
                                    }
                                    else
                                    {
                                        MarkNodeAs(entry, NodeColors.Namespace);
                                    }
                                }
                            }

                            _CurrentIndex = _OriginalNodes.IndexOf(peekedNode);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if (state == 1)
                {
                    if (valid_identifiers.Contains(peekedNode.ClassificationType))
                    {
                        state = 0;
                        indexAhead++;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return false;
    }

    private List<List<Node>> SplitGenericNodes(List<Node> chainElements)
    {
        var output = new List<List<Node>>();
        var tmp = new List<Node>();

        for (int i = 0; i < chainElements.Count; i++)
        {
            Node? node = chainElements[i];
            tmp.Add(node);

            if (node.Text == "," || i == chainElements.Count - 1)
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

    internal bool TryConsumeInheritanceList()
    {
        var previousIsSemicolon = false;

        if (TryPeekBehind(out var peekedBehind))
        {
            if (peekedBehind.ClassificationType == ClassificationTypeNames.LabelName)
                return false;

            previousIsSemicolon = peekedBehind.Text == ":";
        }

        var currentIsSemicolon = CurrentText == ":";

        if (currentIsSemicolon)
        {
            MarkNodeAs(NodeColors.Punctuation);
            MoveNext();
        }

        if (previousIsSemicolon || currentIsSemicolon)
        {
            if (!CheckIfThereIsClassBefore(2))
            {
                // we already consumed punctuation
                if (currentIsSemicolon)
                {
                    MoveBehind();
                    return true;
                }

                return false;
            }

            var validIdentifiers = new List<string>()
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordClassName,
                ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.NamespaceName,
            };

            if (!validIdentifiers.Contains(CurrentNode.ClassificationType)) {
                return false;
            }

            const int STATE_IDENTIFIER = 0;
            const int STATE_COMMA = 1;
            var state = STATE_IDENTIFIER;

            do
            {
                if (state == STATE_IDENTIFIER)
                {
                    if (!validIdentifiers.Contains(CurrentNode.ClassificationType))
                        return false;

                    HandleTypeNameAhead(true);
                    state = STATE_COMMA;
                    continue;
                }
                else
                {
                    if (CurrentText == ",")
                    {
                        MarkNodeAs(NodeColors.Punctuation);
                        state = STATE_IDENTIFIER;
                        continue;
                    }

                    return false;
                }
            } while (MoveNext());
        }

        return false;
    }
}
