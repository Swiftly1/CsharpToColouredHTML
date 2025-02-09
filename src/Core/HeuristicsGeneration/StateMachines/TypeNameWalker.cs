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
        ValueTupleClosing
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
            ClassificationTypeNames.RecordStructName
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
                        if (peekedNode.Text == ".")
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
            x.ClassificationType == ClassificationTypeNames.Operator
        );

        if (!allElementsAreValid)
            return false;

        var identifiers = chainWithoutLastElement
                          .Where(x => TypeHasValidIdentifier(x))
                          .ToList();

        var operators = chainWithoutLastElement
                          .Where(x => x.ClassificationType == ClassificationTypeNames.Operator)
                          .ToList();

        if (!identifiers.Any())
            return false;

        if (identifiers.Count > 1 && operators.Count != identifiers.Count - 1)
            return false;

        // everything is OK, just do not mark nodes
        if (!markIt)
            return true;

        foreach (var node in chainWithoutLastElement)
        {
            if (node.ClassificationType == ClassificationTypeNames.Operator)
            {
                MarkNodeAs(node, NodeColors.Operator);
            }
            else if (node.Id == identifiers[^1].Id)
            {
                var colour = ResolveName(node.Text);
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
            if (TypeHasValidIdentifier(last) && TypeHasValidIdentifier(previous))
                valueTupleTypeWithName = true;
        }

        foreach (var node in chainElements)
        {
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
                    var color = ResolveClassOrStructName(node.Text);
                    MarkNodeAs(node, color);
                }
                else
                {
                    MarkNodeAs(node, NodeColors.Namespace);
                }
            }
            else if (!valueTupleTypeWithName && node.Id == identifiers[^1].Id)
            {
                var color = ResolveClassOrStructName(node.Text);
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

            // 0 = currently at Identifier, expecting Punctuation ","
            // 1 = currently at Punctuation, expecting Identifier

            var state = 0;
            var indexAhead = 1;

            var chainElements = new List<Node> { CurrentNode };

            while (TryPeekAhead(out var peekedNode, indexAhead))
            {
                chainElements.Add(peekedNode);
                if (state == 0)
                {
                    if (peekedNode.Text == ",")
                    {
                        state = 1;
                        indexAhead++;
                        continue;
                    }
                    else
                    {
                        if (peekedNode.Text == ">")
                        {
                            foreach (var entry in chainElements)
                            {
                                if (entry.ClassificationType == ClassificationTypeNames.Punctuation)
                                {
                                    MarkNodeAs(entry, ClassificationTypeNames.Punctuation);
                                }
                                else
                                {
                                    var colour = ResolveName(entry.Text);
                                    MarkNodeAs(entry, colour);
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
}
