using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private void AddClass(string s) => _FoundClasses.Add(s);
    private void AddVariable(string s) => _FoundPropertiesOrFields.Add(s);

    private bool IsValidClassOrStructName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.Skip(1).All(x => char.IsLetter(x) || char.IsNumber(x));
    }

    private bool IdentifierFirstCharCaseSeemsLikeVariable(string s)
    {
        if (s.Length > 0 && char.IsLower(s[0]))
            return true;

        if (s.Length > 0 && s[0] == '_')
            return true;

        return false;
    }

    private bool IsPopularClass(string text)
    {
        return _Hints.ReallyPopularClasses.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase))
            ||
            _Hints.ReallyPopularClassSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPopularStruct(string text)
    {
        return _Hints.ReallyPopularStructs.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase))
            ||
            _Hints.ReallyPopularStructsSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private bool SeemsLikeMethodParameter(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var node = nodes[currentIndex];

        if (!canGoBehind || !canGoAhead)
            return false;

        if (!nodes[currentIndex - 1].Text.EqualsAnyOf(",", "("))
            return false;

        if (nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.ParameterName &&
            nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if (!nodes.CanGoAhead(currentIndex, 2))
            return false;

        if (nodes[currentIndex + 2].Text.EqualsAnyOf(",", ")"))
            return true;

        if (nodes[currentIndex + 1].Text == "[" &&
            nodes[currentIndex + 2].Text == "]" &&
            nodes[currentIndex + 4].Text.EqualsAnyOf(",", ")"))
            return true;

        return false;
    }

    private bool SeemsLikeCast(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var node = nodes[currentIndex];

        if (!canGoBehind || !canGoAhead)
            return false;

        if (nodes[currentIndex - 1].Text != "(")
            return false;

        if (nodes[currentIndex + 1].Text != ")")
            return false;

        if (nodes.Count > currentIndex + 2 && nodes[currentIndex + 2].Text == "new" &&
            nodes[currentIndex + 2].ClassificationType == ClassificationTypeNames.Keyword)
        {
            return false;
        }

        var invalidTypes = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.Keyword
        };

        if (nodes.Count > currentIndex + 2 && !invalidTypes.Contains(nodes[currentIndex + 2].ClassificationType))
            return false;

        if (nodes.Count > currentIndex + 2 &&
            nodes[currentIndex + 2].ClassificationType == ClassificationTypeNames.Keyword &&
            nodes[currentIndex + 2].Text != "this")
        {
            return false;
        }

        return true;
    }

    private bool IsOnInitializationList(int currentIndex, List<Node> nodes)
    {
        // 0045.txt
        if (_IsNew)
        {
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                var suspectedNode = nodes[i];

                if (suspectedNode.Text == "{")
                    return true;

                if (suspectedNode.Text == "new" && suspectedNode.ClassificationType == ClassificationTypeNames.Keyword)
                    break;
            }
        }

        return false;
    }

    private bool IsParameterWithAttribute(int currentIndex, List<Node> nodes)
    {
        if (!nodes.CanGoBehind(currentIndex, 4))
            return false;

        var closing = nodes[currentIndex - 1];
        var name = nodes[currentIndex - 2];
        var opening = nodes[currentIndex - 3];
        var commaOrParenthesis = nodes[currentIndex - 4];

        if (closing.Text != "]" || closing.ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if (opening.Text != "[" || opening.ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if (commaOrParenthesis.Text != "(" && commaOrParenthesis.Text != ","
             || commaOrParenthesis.ClassificationType != ClassificationTypeNames.Punctuation)
        {
            return false;
        }

        return IsValidClassOrStructName(name.Text);
    }

    private bool RightSideOfAssignmentHasTheSameNameAfterNew(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        if (IsOnInitializationList(currentIndex, nodes))
            return false;

        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        if (canGoBehind && nodes[currentIndex - 1].Text == ".")
            return false;

        if (nodes.Count > currentIndex + 4 &&
                    nodes[currentIndex + 2].Text == "=" &&
                    nodes[currentIndex + 3].Text == "new" &&
                    nodes[currentIndex + 4].Text == node.Text)
        {
            return true;
        }

        //ConcurrentDictionary<int, Action> allJobs = new ConcurrentDictionary<int, Action>();

        if (nodes.Count == 1)
            return true;

        if (currentIndex >= 2 && canGoAhead && nodes[currentIndex + 1].Text == "." && nodes[currentIndex - 1].Text == ".")
        {
            var classifications = new[]
            {
                ClassificationTypeNames.Keyword, ClassificationTypeNames.LocalName,
                ClassificationTypeNames.PropertyName, ClassificationTypeNames.FieldName,
                ClassificationTypeNames.ConstantName, ClassificationTypeNames.ParameterName
            };

            if (classifications.Contains(nodes[currentIndex - 2].ClassificationType))
                return false;
        }

        for (int i = currentIndex + 1; i < nodes.Count; i++)
        {
            var current = nodes[i];

            if (current.ClassificationType == ClassificationTypeNames.Punctuation && current.Text == ";")
                return false;

            if (current.ClassificationType == ClassificationTypeNames.ClassName && current.Text == node.Text && nodes[i - 1].Text == "new")
                return true;

            if (current.ClassificationType == ClassificationTypeNames.Identifier && current.Text == node.Text
                && nodes[i - 1].Text == "new")
            {
                // case like this: = new Test.ABC();
                // "Test" shouldn't be a class here

                // if next node is "."
                return i + 1 < nodes.Count && nodes[i + 1].Text == "." ? false : true;
            }
        }

        return false;
    }

    private bool SeemsLikePropertyUsage(int currentIndex, List<Node> nodes)
    {
        if (_IsUsing || _IsTypeOf)
            return false;

        if (currentIndex >= 2 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].Text == "this" &&
            nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.Keyword)
        {
            return false;
        }

        if (!nodes.CanGoAhead(currentIndex, 3))
            return false;

        var next = nodes[currentIndex + 1];

        if (next.ClassificationType != ClassificationTypeNames.Operator || next.Text == "=")
            return false;

        next = nodes[currentIndex + 2];

        if (next.ClassificationType != ClassificationTypeNames.Identifier)
            return false;

        next = nodes[currentIndex + 3];

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.LocalName)
            return false;

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.ParameterName)
            return false;

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.Identifier)
        {
            if (char.IsLower(nodes[currentIndex - 2].Text[0]))
            {
                return false;
            }
        }

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].Text == ">")
            return false;

        var comesFromVariable = ThereIsVariableInTheChainBefore(currentIndex, nodes);

        if (comesFromVariable)
            return false;

        var validTypes = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.ConstantName
        };

        // seems like a cast
        if (currentIndex >= 1 && nodes[currentIndex - 1].Text == "(" &&
            currentIndex + 4 < nodes.Count &&
            nodes[currentIndex + 3].Text == ")" &&
            validTypes.Contains(nodes[currentIndex + 4].ClassificationType))
        {
            return false;
        }

        // OLEMSGICON.OLEMSGICON_WARNING,
        return next.Text.EqualsAnyOf(")", "=", ";", "}", ",", "&",
                              "&&", "|", "||", "+", "-", "*",
                              "/", "-=", "+=", "*=", "/=", "%=",
                               "&=", "|=", "^=", ">>=", "<<=");
    }


    private bool ThereIsMethodCallAhead(int currentIndex, List<Node> nodes)
    {
        var validTypes = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.Operator,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.Punctuation
        };

        Func<Node, bool> func = x => x.ClassificationType == ClassificationTypeNames.Punctuation && x.Text == "(";

        if (ThereIsSpecificItemInTheChainAhead(currentIndex, nodes, func, validTypes))
        {
            return true;
        }

        return false;
    }

    private bool ThereIsVariableInTheChainBefore(int currentIndex, List<Node> nodes)
    {
        var validIdentifiers = new[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
        };

        return ThereIsSpecificItemInTheChainBefore(currentIndex, nodes, x => validIdentifiers.Contains(x.ClassificationType));
    }

    private bool ThereIsThisInTheChainBefore(int currentIndex, List<Node> nodes)
    {
        var validIdentifiers = new[]
        {
            ClassificationTypeNames.Keyword,
        };

        Func<Node, bool> func = x => validIdentifiers.Contains(x.ClassificationType) && x.Text == "this";

        return ThereIsSpecificItemInTheChainBefore(currentIndex, nodes, func);
    }

    private static bool ThereIsSpecificItemInTheChainBefore(int currentIndex, List<Node> nodes, Func<Node, bool> func)
    {
        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier
        var state = 0;

        for (int i = currentIndex - 1; i >= 0; i--)
        {
            if (state == 0)
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Operator && nodes[i].Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            else if (state == 1)
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Identifier)
                {
                    state = 0;
                    continue;
                }
                else
                {
                    // if it is Local/Constant/Param then return that there's variable before
                    return func(nodes[i]);
                }
            }
        }

        return false;
    }

    private static bool ThereIsSpecificItemInTheChainAhead(int currentIndex, List<Node> nodes, Func<Node, bool> func, List<string> validIdentifiers)
    {
        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier
        var state = 0;

        for (int i = currentIndex + 1; i < nodes.Count; i++)
        {
            var current = nodes[i];
            if (state == 0)
            {
                if (current.ClassificationType == ClassificationTypeNames.Operator && current.Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    return func(nodes[i]);
                }
            }
            else if (state == 1)
            {
                if (validIdentifiers.Contains(current.ClassificationType))
                {
                    state = 0;
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

    private bool ThereIsClassInTheChainBefore(NodeWithDetails entry, List<NodeWithDetails> nodes)
    {
        var currentIndex = nodes.IndexOf(entry);
        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier
        var state = 0;

        for (int i = currentIndex - 1; i >= 0; i--)
        {
            if (state == 0)
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Operator && nodes[i].Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            else if (state == 1)
            {
                var validColors = new List<string>
                {
                    NodeColors.FieldName,
                    NodeColors.PropertyName,
                    NodeColors.LocalName,
                    NodeColors.ParameterName,
                    NodeColors.ConstantName,
                    NodeColors.Identifier,
                };

                if (validColors.Contains(nodes[i].Colour))
                {
                    state = 0;
                    continue;
                }
                else
                {
                    if (nodes[i].Colour == NodeColors.Class)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;
    }

    private bool CheckIfChainIsMadeOfVariablesColors(int currentIndex, List<NodeWithDetails> nodes)
    {
        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var identifiers = new List<string>();

        var validIdentifiers = new[]
        {
            NodeColors.LocalName,
            NodeColors.ConstantName,
            NodeColors.ParameterName,
            NodeColors.PropertyName,
            NodeColors.FieldName
        };

        var validTypes = new[]
        {
            NodeColors.LocalName,
            NodeColors.ConstantName,
            NodeColors.ParameterName,
            NodeColors.PropertyName,
            NodeColors.FieldName,

            NodeColors.Identifier,
            NodeColors.Operator,
        };

        for (int i = currentIndex - 1; i >= 0; i--)
        {
            if (!validTypes.Contains(nodes[i].Colour))
            {
                if (nodes[i].Colour == NodeColors.Punctuation && nodes[i].Text == ">")
                {
                    return false;
                }

                break;
            }

            if (state == 0)
            {
                if (nodes[i].Colour == NodeColors.Operator && nodes[i].Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else if (state == 1)
            {
                if (nodes[i].Colour == NodeColors.Identifier)
                {
                    identifiers.Add(nodes[i].Text);
                    state = 0;
                    continue;
                }
                else
                {
                    // if it is Local/Constant/Param then halt.
                    if (validIdentifiers.Contains(nodes[i].Colour))
                    {
                        return false;
                    }

                    break;
                }
            }
        }

        if (identifiers.Count > 0 && identifiers.All(x => !IdentifierFirstCharCaseSeemsLikeVariable(x)))
            return true;

        return false;
    }

    private void TryUpdatePreviousIdentifierToClassIfThatWasNamespace(int currentIndex, List<Node> nodes)
    {
        if (!nodes.CanGoBehind(currentIndex, 2))
            return;

        if (nodes[currentIndex - 1].ClassificationType != ClassificationTypeNames.Operator)
            return;

        var suspectedNode = nodes[currentIndex - 2];

        if (suspectedNode.ClassificationType != ClassificationTypeNames.Identifier)
            return;

        var alreadyProcessedSuspectedNode = _Output.LastOrDefault(x => x.Id == suspectedNode.Id && x.Colour == NodeColors.Identifier);

        if (alreadyProcessedSuspectedNode == null)
            return;

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;
        var identifiers = new List<string>();

        var validIdentifiers = new[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName
        };

        var validTypes = new[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.Operator,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.FieldName
        };

        for (int i = currentIndex - 3; i >= 0; i--)
        {
            if (!validTypes.Contains(nodes[i].ClassificationType))
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == ">")
                    return;

                if (nodes[i].ClassificationType == ClassificationTypeNames.Keyword && nodes[i].Text == "this")
                    return;

                if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == ")")
                {
                    var closed_counter = 0;

                    for (; i >= 0; i--)
                    {
                        if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == ")")
                            closed_counter++;

                        if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == "(")
                            closed_counter--;

                        if (closed_counter <= 0)
                            break;
                    }

                    continue;
                }

                break;
            }

            if (state == 0)
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Operator && nodes[i].Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else if (state == 1)
            {
                if (nodes[i].ClassificationType == ClassificationTypeNames.Identifier)
                {
                    identifiers.Add(nodes[i].Text);
                    state = 0;
                    continue;
                }
                else
                {
                    // if it is Local/Constant/Param then halt.
                    if (validIdentifiers.Contains(nodes[i].ClassificationType))
                    {
                        return;
                    }

                    break;
                }
            }
        }

        if (identifiers.Count == 0)
        {
            var isVariable = IdentifierFirstCharCaseSeemsLikeVariable(alreadyProcessedSuspectedNode.Text);

            if (isVariable)
            {
                alreadyProcessedSuspectedNode.Colour = NodeColors.LocalName;
            }
            else
            {
                AddClass(alreadyProcessedSuspectedNode.Text);
                alreadyProcessedSuspectedNode.Colour = NodeColors.Class;
            }
        }

        if (identifiers.Count > 0 && identifiers.All(x => !IdentifierFirstCharCaseSeemsLikeVariable(x)))
        {
            if (identifiers.Any(i => _FoundClasses.Contains(i)))
            {
                if (alreadyProcessedSuspectedNode.Colour == NodeColors.Identifier)
                {
                    alreadyProcessedSuspectedNode.Colour = NodeColors.PropertyName;
                }

                return;
            }

            AddClass(alreadyProcessedSuspectedNode.Text);
            alreadyProcessedSuspectedNode.Colour = NodeColors.Class;
        }
    }
}