using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private bool CheckIfThereIsNewBeforeMethodCall()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.MethodName,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ParameterName,
        };

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var indexBehind = 1;

        while (TryPeekBehind(out var peekedNode, indexBehind))
        {
            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexBehind++;
                    continue;
                }
                else
                {
                    return peekedNode.Text == "new";
                }
            }
            else if (state == 1)
            {
                var outputNode = _Output[_CurrentIndex - indexBehind];
                if (valid_identifiers.Contains(outputNode.ClassificationType))
                {
                    state = 0;
                    indexBehind++;
                    continue;
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
    }

    private bool TryReadNamespaceChain()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName
        };

        // 0 = currently at Identifier, expecting Operator "." or ";"
        // 1 = currently at Operator, expecting Identifier

        var state = 1;
        var indexAhead = 1;

        var chainElements = new List<Node>();

        while (TryPeekAhead(out var peekedNode, indexAhead))
        {
            chainElements.Add(peekedNode);
            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexAhead++;
                    continue;
                }
                else
                {
                    if (peekedNode.Text == ";")
                    {
                        var chainWithoutLastElement = chainElements.SkipLast(1).ToList();
                        foreach (var entry in chainWithoutLastElement)
                        {
                            var colour = entry.ClassificationType == ClassificationTypeNames.Operator ?
                                         NodeColors.Operator :
                                         NodeColors.Namespace;

                            if (colour == NodeColors.Namespace)
                                _FoundNamespaceParts.Add(entry.Text);

                            MarkNodeAs(entry, colour);
                        }

                        MarkNodeAs(peekedNode, NodeColors.Punctuation);
                        _FoundNamespaces.Add(string.Join("", chainElements.SkipLast(1).Select(x => x.Text).ToList()));
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

        return false;
    }

    private bool TryReadIdentifierChain()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName
        };

        // 0 = currently at Identifier, expecting Operator "." or "("
        // 1 = currently at Operator, expecting Identifier

        var state = 1;
        var indexAhead = 1;

        var chainElements = new List<Node>();

        while (TryPeekAhead(out var peekedNode, indexAhead))
        {
            chainElements.Add(peekedNode);
            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexAhead++;
                    continue;
                }
                else
                {
                    if (peekedNode.Text == "(")
                    {
                        if (chainElements.Count < 2)
                            return false;

                        var chainWithoutLastElements = chainElements.SkipLast(2).ToList();
                        foreach (var entry in chainWithoutLastElements)
                        {
                            var entryColour = entry.ClassificationType == ClassificationTypeNames.Operator ?
                                         NodeColors.Operator :
                                         NodeColors.Namespace;

                            MarkNodeAs(entry, entryColour);
                        }

                        var objIdentifierNode = chainElements[^2];
                        var objIdentifierNodeColour = ResolveName(objIdentifierNode.Text);

                        MarkNodeAs(objIdentifierNode, objIdentifierNodeColour);
                        MarkNodeAs(peekedNode, NodeColors.Punctuation);

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

        return false;
    }

    private bool CheckClassPropertiesUsageChain()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName,
        };

        if (!valid_identifiers.Contains(CurrentNode.ClassificationType))
            return false;

        if (TryPeekAhead(out var nodeAhead) && nodeAhead.Text != ".")
            return false;

        if (TryPeekBehind(out var nodeBehind) && nodeBehind.Text == "." &&
            TryPeekBehind(out var nodeBehind2, 2) && !valid_identifiers.Contains(nodeBehind2.ClassificationType))
            return false;

        if (CheckIfThereIsLocalNameOrPropertyBefore())
            return false;

        // 0 = currently at Identifier, expecting Operator "."
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var indexAhead = 1;

        var chainElements = new List<Node> { CurrentNode };

        while (TryPeekAhead(out var peekedNode, indexAhead))
        {
            chainElements.Add(peekedNode);
            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexAhead++;
                    continue;
                }
                else
                {
                    var allElementsAreValid = chainElements.All(x =>
                        valid_identifiers.Contains(x.ClassificationType) ||
                        x.ClassificationType == ClassificationTypeNames.Operator ||
                        x.ClassificationType == ClassificationTypeNames.Punctuation
                    );

                    if (!allElementsAreValid) return false;

                    var chainWithoutLastElement = chainElements.SkipLast(1).ToList();

                    var identifiers = chainWithoutLastElement
                                      .Where(x => valid_identifiers.Contains(x.ClassificationType))
                                      .ToList();

                    var operators = chainWithoutLastElement
                                      .Where(x => x.ClassificationType == ClassificationTypeNames.Operator)
                                      .ToList();

                    if (!identifiers.Any())
                        return false;

                    if (identifiers.Count > 1 &&
                        operators.Count != identifiers.Count - 1)
                        return false;

                    var classAlreadyUsed = false;
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
                        else if (node.Id == identifiers[^1].Id)
                        {
                            if (peekedNode.Text == "(")
                                MarkNodeAs(node, NodeColors.Method);
                            else if (peekedNode.Text == "<")
                            {
                                if (LookAheadWhatIsAfterGenerics(peekedNode, out var nodeAfterGenerics))
                                {
                                    if (nodeAfterGenerics.Text == "(")
                                        MarkNodeAs(node, NodeColors.Method);
                                    else if (nodeAfterGenerics.Text == ".")
                                        MarkNodeAs(node, NodeColors.PropertyName);
                                }
                                else
                                {
                                    MarkNodeAs(node, NodeColors.PropertyName);
                                }
                            }
                            else
                                MarkNodeAs(node, NodeColors.PropertyName);
                        }
                        else if (identifiers.Count > 1 && node.Id == identifiers[^2].Id)
                        {
                            if (classAlreadyUsed)
                            {
                                MarkNodeAs(node, NodeColors.PropertyName);
                            }
                            else
                            {
                                var colour = ResolveName(node.Text);
                                MarkNodeAs(node, colour);
                            }
                        }
                        else
                        {
                            if (IsClassOrStructAlreadyFound(node.Text))
                            {
                                var colour = ResolveName(node.Text);
                                MarkNodeAs(node, colour);
                                classAlreadyUsed = true;
                            }
                            else
                            {
                                MarkNodeAs(node, NodeColors.Namespace);
                            }
                        }
                    }

                    _CurrentIndex = _OriginalNodes.IndexOf(peekedNode);
                    return true;
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

        return false;
    }

    private bool LookAheadWhatIsAfterGenerics(Node peekedNode, out Node foundNode)
    {
        foundNode = null!;
        if (peekedNode.Text != "<")
            return false;

        var index = _OriginalNodes.IndexOf(peekedNode) + 1;
        var openAngleCounter = 1;

        while (TryPeekAtIndex(out var nextNode, index))
        {
            if (nextNode.Text == "<")
            {
                openAngleCounter++;
            }
            else if (nextNode.Text == ">")
            {
                openAngleCounter--;
            }
            else if (nextNode.Text == ";")
            {
                return false;
            }
            
            if (openAngleCounter == 0)
            {
                return TryPeekAtIndex(out foundNode, index + 1);
            }

            index++;
        }

        return false;
    }

    private bool CheckIfThereIsLocalNameOrPropertyBefore()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
        };

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var indexBehind = 1;

        while (TryPeekBehind(out var peekedNode, indexBehind))
        {
            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexBehind++;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            else if (state == 1)
            {
                var outputNode = _Output[_CurrentIndex - indexBehind];
                if (valid_identifiers.Contains(outputNode.ClassificationType))
                {
                    return true;
                }

                if (outputNode.ClassificationType == ClassificationTypeNames.Identifier)
                {
                    state = 0;
                    indexBehind++;
                    continue;
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
    }

    private bool TryDetectCastAhead()
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.Identifier,
        };

        var valid_cast_variables = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
        };

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var indexAhead = 1;
        var chainElements = new List<Node> { CurrentNode };

        while (TryPeekAhead(out var peekedNode, indexAhead))
        {
            chainElements.Add(peekedNode);

            if (state == 0)
            {
                if (peekedNode.Text == ".")
                {
                    state = 1;
                    indexAhead++;
                    continue;
                }
                else
                {
                    if (peekedNode.Text == ")")
                    {
                        if (TryPeekAhead(out var peekAgain, indexAhead + 1) &&
                            valid_cast_variables.Contains(peekAgain.ClassificationType))
                        {
                            var identifiers = chainElements
                                      .Where(x => valid_identifiers.Contains(x.ClassificationType))
                                      .ToList();

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
                                else if (node.Id == identifiers[^1].Id)
                                {
                                    MarkNodeAs(node, NodeColors.Class);
                                }
                                else
                                {
                                    MarkNodeAs(node, NodeColors.Namespace);
                                }
                            }
                            _CurrentIndex = _OriginalNodes.IndexOf(peekedNode);
                            return true;
                        }

                        return false;
                    }
                    return false;
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

        return false;
    }
}