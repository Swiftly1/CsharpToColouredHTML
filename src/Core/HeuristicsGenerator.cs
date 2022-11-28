using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

internal class HeuristicsGenerator
{
    private bool _IsUsing = false;
    private bool _IsTypeOf = false;
    // Simplifies detecting creation of an instance, so we don't have to go behind.
    // So far it works decent, thus no need for more complex approach.
    private bool _IsNew = false;

    private int _ParenthesisCounter = 0;

    private readonly Hints _Hints;

    private List<NodeWithDetails> _Output = new List<NodeWithDetails>();

    private List<string> _FoundClasses = new List<string>();
    private List<string> _FoundStructs = new List<string>();
    private List<string> _FoundInterfaces = new List<string>();

    private Dictionary<string, string> _SimpleClassificationToColourMapper { get; } = new()
    {
        { ClassificationTypeNames.ClassName, NodeColors.Class },
        { ClassificationTypeNames.Comment, NodeColors.Comment },
        { ClassificationTypeNames.PreprocessorKeyword, NodeColors.Preprocessor },
        { ClassificationTypeNames.PreprocessorText, NodeColors.PreprocessorText },
        { ClassificationTypeNames.StructName, NodeColors.Struct },
        { ClassificationTypeNames.InterfaceName, NodeColors.Interface },
        { ClassificationTypeNames.NamespaceName, NodeColors.Namespace },
        { ClassificationTypeNames.EnumName, NodeColors.EnumName },
        { ClassificationTypeNames.EnumMemberName, NodeColors.EnumMemberName },
        { ClassificationTypeNames.StringLiteral, NodeColors.String },
        { ClassificationTypeNames.VerbatimStringLiteral, NodeColors.String },
        { ClassificationTypeNames.LocalName, NodeColors.LocalName },
        { ClassificationTypeNames.MethodName, NodeColors.Method },
        { ClassificationTypeNames.Operator, NodeColors.Operator },
        { ClassificationTypeNames.PropertyName, NodeColors.PropertyName },
        { ClassificationTypeNames.ParameterName, NodeColors.ParameterName },
        { ClassificationTypeNames.FieldName, NodeColors.FieldName },
        { ClassificationTypeNames.NumericLiteral, NodeColors.NumericLiteral },
        { ClassificationTypeNames.ControlKeyword, NodeColors.Control },
        { ClassificationTypeNames.LabelName, NodeColors.LabelName },
        { ClassificationTypeNames.OperatorOverloaded, NodeColors.OperatorOverloaded },
        { ClassificationTypeNames.RecordStructName, NodeColors.RecordStructName },
        { ClassificationTypeNames.RecordClassName, NodeColors.Class },
        { ClassificationTypeNames.TypeParameterName, NodeColors.TypeParameterName },
        { ClassificationTypeNames.ExtensionMethodName, NodeColors.ExtensionMethodName },
        { ClassificationTypeNames.ConstantName, NodeColors.ConstantName },
        { ClassificationTypeNames.DelegateName, NodeColors.Delegate },
        { ClassificationTypeNames.EventName, NodeColors.EventName },
        { ClassificationTypeNames.ExcludedCode, NodeColors.ExcludedCode },
    };

    public HeuristicsGenerator(Hints hints)
    {
        _Hints = hints;
    }

    public List<NodeWithDetails> Build(List<Node> input)
    {
        ProcessData(input);
        PostProcess(_Output);
        return _Output;
    }

    public void Reset()
    {
        _FoundClasses.Clear();
        _FoundInterfaces.Clear();
        _FoundStructs.Clear();
        _Output.Clear();
        _IsUsing = false;
        _IsNew = false;
        _IsTypeOf = false;
        _ParenthesisCounter = 0;
    }

    private void ProcessData(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var colour = ExtractColourAndSetMetaData(i, nodes);
            var nodeWithDetails = new NodeWithDetails
            (
                colour: colour,
                text: nodes[i].Text,
                trivia: nodes[i].Trivia,
                hasNewLine: nodes[i].HasNewLine,
                isNew: _IsNew,
                isUsing: _IsUsing,
                parenthesisCounter: _ParenthesisCounter,
                classificationType: nodes[i].ClassificationType,
                id: nodes[i].Id
            );
            _Output.Add(nodeWithDetails);
        }
    }

    private void PostProcess(List<NodeWithDetails> alreadyProcessed)
    {
        // If some identifiers weren't recognized at first attempt, but later instead
        // then we may fix the previous ones.
        var identifiers = alreadyProcessed.Where(x => x.Colour == NodeColors.Identifier && x.IsUsing == false).ToList();

        foreach (var entry in identifiers)
        {
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
    }

    private string ExtractColourAndSetMetaData(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var colour = NodeColors.InternalError;

        if (_SimpleClassificationToColourMapper.TryGetValue(node.ClassificationType, out var simpleColour))
            return simpleColour;

        if (_Hints.BuiltInTypes.Contains(node.Text))
        {
            _IsNew = false;
            colour = NodeColors.Keyword;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            if (IsInterface(currentIndex, nodes))
            {
                colour = NodeColors.Interface;
                _FoundInterfaces.Add(node.Text);
            }
            else if (IsMethod(currentIndex, nodes))
            {
                colour = NodeColors.Method;

                TryUpdatePreviousIdentifierToClassIfThatWasNamespace(currentIndex, nodes);
            }
            else if (IsClassOrStruct(currentIndex, nodes))
            {
                if (IsPopularStruct(node.Text))
                {
                    colour = NodeColors.Struct;
                    _FoundStructs.Add(node.Text);
                }
                else if (IdentifierFirstCharCaseSeemsLikeVariable(node.Text))
                {
                    colour = NodeColors.LocalName;
                }
                else
                {
                    colour = NodeColors.Class;
                    _FoundClasses.Add(node.Text);
                }
            }
            else
            {
                colour = NodeColors.Identifier;

                if (currentIndex + 1 < nodes.Count &&
                    nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.Operator &&
                    nodes[currentIndex + 1].Text != "." &&
                    !_IsUsing)
                {
                    if (CheckIfChainIsMadeOfVariablesColors(currentIndex, _Output))
                    {
                        colour = NodeColors.Class;
                        _FoundClasses.Add(node.Text);
                    }
                }
            }
        }
        else if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            if (node.Text == "using")
                _IsUsing = true;

            if (node.Text == "new")
                _IsNew = true;

            if (node.Text == "typeof")
                _IsTypeOf = true;

            colour = NodeColors.Keyword;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            if (node.Text == "(")
                _ParenthesisCounter++;

            if (node.Text == ")")
            {
                _ParenthesisCounter--;

                if (_ParenthesisCounter <= 0 && _IsUsing)
                    _IsUsing = false;

                if (_ParenthesisCounter <= 0 && _IsNew)
                    _IsNew = false;

                if (_ParenthesisCounter <= 0 && _IsTypeOf)
                    _IsTypeOf = false;
            }

            if (node.Text == ";")
            {
                _IsUsing = false;
                _IsNew = false;
                _IsTypeOf = false;
            }

            colour = NodeColors.Punctuation;
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            colour = NodeColors.Comment;
        }

        return colour;
    }

    private bool IsInterface(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoTwoAhead = nodes.Count > currentIndex + 2;
        var canGoBehind = currentIndex > 0;

        var startsWithI = node.Text.StartsWith("I");

        if (startsWithI && canGoBehind && new[] { ":", "<" }.Contains(nodes[currentIndex - 1].Text))
        {
            return true;
        }
        else if (startsWithI && canGoBehind && new[] { "public", "private", "internal", "sealed", "protected", "readonly" }.Contains(nodes[currentIndex - 1].Text))
        {
            return true;
        }
        else if (startsWithI && canGoAhead && new[] { ClassificationTypeNames.ParameterName, ClassificationTypeNames.FieldName }.Contains(nodes[currentIndex + 1].ClassificationType))
        {
            return true;
        }
        else if (startsWithI && canGoTwoAhead && nodes[currentIndex + 1].Text == ")" && nodes[currentIndex + 2].Text != "{")
        {
            if (currentIndex >= 2)
            {
                var suspectedId = nodes[currentIndex - 2].Id;
                var suspected = _Output.First(x => x.Id == suspectedId);

                if (nodes[currentIndex - 1].Text == "." && new[] { NodeColors.LocalName, NodeColors.ParameterName }.Contains(suspected.Colour))
                    return false;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsMethod(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        // [InlineData("0001.txt")]
        // var a = list[test()];
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "[")
        {
            var identifier = nodes[currentIndex - 2].ClassificationType;

            var hasIdentifierBefore = identifier == ClassificationTypeNames.Identifier ||
                identifier == ClassificationTypeNames.LocalName;

            if (!hasIdentifierBefore)
                return false;
        }

        if (!_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return true;
        }
        else if (_IsUsing && !_IsUsing && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return true;
        }
        // simple generics
        else if (!_IsNew && currentIndex + 4 < nodes.Count && nodes[currentIndex + 1].Text == "<"
            && nodes[currentIndex + 3].Text == ">" && nodes[currentIndex + 4].Text == "(")
        {
            var validTypes = new[] 
            {
                ClassificationTypeNames.ClassName, ClassificationTypeNames.StructName, 
                ClassificationTypeNames.RecordClassName, ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.Identifier 
            };
            
            return validTypes.Contains(nodes[currentIndex + 2].ClassificationType);
        }
        else
        {
            return false;
        }
    }

    private bool IsClassOrStruct(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        var node = nodes[currentIndex];
        bool isPopularClassOrStruct = false;

        if (canGoBehind && nodes[currentIndex - 1].Text == ":")
        {
            return true;
        }
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return true;
        }
        else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
        {
            return true;
        }
        else if (_IsNew && ThereIsMethodCallAhead(currentIndex, nodes))
        {
            _IsNew = false;
            return true;
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return true;
        } // be careful, if you remove those parenthesis around that assignment, then it'll change its behaviour
        else if ((isPopularClassOrStruct = IsPopularClass(node.Text) || IsPopularStruct(node.Text)) && !canGoBehind)
        {
            return true;
        }
        else if (isPopularClassOrStruct && canGoBehind && nodes[currentIndex - 1].Text != ".")
        {
            return true;
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "<")
        {
            return true;
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "[")
        {
            return true;
        }
        // new DictionaryList<int, int>();
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            _IsNew = false;
            return true;
        }
        // public void Test(Array<int> a)
        else if (canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            return true;
        }
        else if (SeemsLikeParameter(currentIndex, nodes))
        {
            return true;
        }
        else if (RightSideOfAssignmentHasTheSameNameAfterNew(currentIndex, nodes))
        {
            return true;
        }
        else if (currentIndex >= 2 && nodes.Count >= 3 && nodes[currentIndex -1].ClassificationType == ClassificationTypeNames.Punctuation &&
            nodes[currentIndex - 1].Text == "(" && nodes[currentIndex - 2].Text == "foreach")
        {
            return true;
        }
        else if (IsParameterWithAttribute(currentIndex, nodes))
        {
            return true;
        }

        return false;
    }

    private bool SeemsLikeParameter(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        var node = nodes[currentIndex];

        if (!canGoBehind || !canGoAhead)
            return false;

        if (!new[] { ",", "(" }.Contains(nodes[currentIndex - 1].Text))
            return false;

        if (nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.ParameterName)
            return false;

        if (currentIndex + 2 >= nodes.Count)
            return false;

        if (new[] { ",", ")" }.Contains(nodes[currentIndex + 2].Text))
            return true;

        return false;
    }

    private bool RightSideOfAssignmentHasTheSameNameAfterNew(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

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

        for (int i = currentIndex + 1; i < nodes.Count; i++)
        {
            var current = nodes[i];

            if (current.ClassificationType == ClassificationTypeNames.Identifier && current.Text == node.Text
                && nodes[i - 1].Text == "new")
            {
                // case like this: = new Test.ABC();
                // "Test" shouldn't be a class here

                // if next node is "."
                return (i + 1 < nodes.Count && nodes[i + 1].Text == ".") ? false : true;
            }
        }

        return false;
    }

    private bool SeemsLikePropertyUsage(int currentIndex, List<Node> nodes)
    {
        if (_IsUsing || _IsTypeOf)
            return false;

        if (currentIndex + 3 >= nodes.Count)
            return false;

        var next = nodes[currentIndex + 1];

        if (next.ClassificationType != ClassificationTypeNames.Operator)
            return false;

        next = nodes[currentIndex + 2];

        if (next.ClassificationType != ClassificationTypeNames.Identifier)
            return false;

        next = nodes[currentIndex + 3];

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.LocalName)
            return false;

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.ParameterName)
            return false;

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].Text == ">")
            return false;

        var comesFromVariable = TheresVariableInTheChainBefore(currentIndex, nodes);

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
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "(" && currentIndex + 4 < nodes.Count && validTypes.Contains(nodes[currentIndex + 4].ClassificationType))
            return false;

        // OLEMSGICON.OLEMSGICON_WARNING,
        return new string[] { ")", "=", ";", "}", ",", "&", "&&", "|", "||" }.Contains(next.Text);
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

    private bool ThereIsMethodCallAhead(int currentIndex, List<Node> nodes)
    {
        if (currentIndex + 1 >= nodes.Count)
            return false;

        var parenthesis = nodes[currentIndex + 1];

        if (parenthesis.ClassificationType == ClassificationTypeNames.Punctuation && parenthesis.Text == "(")
            return true;

        return false;
    }

    private bool IsParameterWithAttribute(int currentIndex, List<Node> nodes)
    {
        if (currentIndex - 4 < 0)
            return false;

        var closing = nodes[currentIndex - 1];
        var name = nodes[currentIndex - 2];
        var opening = nodes[currentIndex - 3];
        var commaOrParenthesis = nodes[currentIndex - 4];

        if (closing.Text != "]" || closing.ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if (opening.Text != "[" || opening.ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if ((commaOrParenthesis.Text != "(" && commaOrParenthesis.Text != ",")
             || commaOrParenthesis.ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        return IsValidClassOrStructName(name.Text);
    }

    private bool TheresVariableInTheChainBefore(int currentIndex, List<Node> nodes)
    {
        var validIdentifiers = new[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.ParameterName,
        };

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
                    if (validIdentifiers.Contains(nodes[i].ClassificationType))
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;
    }

    private void TryUpdatePreviousIdentifierToClassIfThatWasNamespace(int currentIndex, List<Node> nodes)
    {
        if (currentIndex - 2 < 0)
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
                {
                    return;
                }

                if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == ")")
                {
                    var closed_counter = 0;

                    for (; i >= 0 ; i--)
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
                alreadyProcessedSuspectedNode.Colour = NodeColors.Class;
            }
        }

        if (identifiers.Count > 0 && identifiers.All(x => !IdentifierFirstCharCaseSeemsLikeVariable(x)))
            alreadyProcessedSuspectedNode.Colour = NodeColors.Class;
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
        return s.Length > 0 && char.IsLower(s[0]);
    }
}