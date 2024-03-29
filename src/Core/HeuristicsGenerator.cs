﻿using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

internal class HeuristicsGenerator
{
    private bool _IsUsing = false;
    private bool _IsTypeOf = false;
    // Simplifies detecting creation of an instance, so we don't have to go behind.
    // So far it works decent, thus no need for more complex approach.
    private bool _IsNew = false;
    private bool _IsWithinMethod = false;

    private int _ParenthesisCounter = 0;
    private int _BracketsCounter = 0;

    private readonly Hints _Hints;

    private List<NodeWithDetails> _Output = new();

    private HashSet<string> _FoundClasses = new();
    private HashSet<string> _FoundStructs = new();
    private HashSet<string> _FoundInterfaces = new();
    private HashSet<string> _FoundPropertiesOrFields = new();

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

    public List<NodeAfterProcessing> Build(List<Node> input)
    {
        Reset();
        ProcessData(input);
        PostProcess(_Output);
        AssignLineNumbers(_Output);

        return _Output.Select(x => new NodeAfterProcessing
        (
            x.Id,
            x.Colour,
            x.Text,
            x.Trivia,
            x.ClassificationType,
            x.UsesMostCommonColour,
            x.LineNumber,
            useHighlighting: false // it may be defined later by postprocessor
        )).ToList();
    }

    public void Reset()
    {
        _FoundClasses.Clear();
        _FoundInterfaces.Clear();
        _FoundStructs.Clear();
        _FoundPropertiesOrFields.Clear();
        _Output.Clear();
        _IsUsing = false;
        _IsNew = false;
        _IsTypeOf = false;
        _IsWithinMethod = false;
        _ParenthesisCounter = 0;
        _BracketsCounter = 0;
    }

    private void AssignLineNumbers(List<NodeWithDetails> output)
    {
        var currentLineNumber = 0;

        foreach (var node in output)
        {
            if (node.HasNewLine)
            {
                var newLinesCount = StringHelper.AllIndicesOf(node.Trivia, Environment.NewLine).Count;
                currentLineNumber += newLinesCount;
            }

            node.LineNumber = currentLineNumber;
        }
    }

    private void ProcessData(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var result = ExtractColourAndSetMetaData(i, nodes);

            var nodeWithDetails = new NodeWithDetails
            (
                colour: result.Colour,
                text: nodes[i].Text,
                trivia: nodes[i].Trivia,
                hasNewLine: nodes[i].HasNewLine,
                isNew: _IsNew,
                isUsing: _IsUsing,
                parenthesisCounter: _ParenthesisCounter,
                classificationType: nodes[i].ClassificationType,
                id: nodes[i].Id,
                skipIdentifierPostProcessing: result.SkipIdentifierPostProcessing
            );

            _Output.Add(nodeWithDetails);
        }
    }

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

    private ExtractedColourResult ExtractColourAndSetMetaData(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        if (_SimpleClassificationToColourMapper.TryGetValue(node.ClassificationType, out var simpleColour))
            return new ExtractedColourResult(simpleColour);

        if (_Hints.BuiltInTypes.Contains(node.Text))
        {
            _IsNew = false;
            return new ExtractedColourResult(NodeColors.Keyword);
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            var isInterfaceResult = IsInterface(currentIndex, nodes);
            if (isInterfaceResult.Detected)
            {
                _FoundInterfaces.Add(node.Text);
                return new ExtractedColourResult(NodeColors.Interface, isInterfaceResult.SkipPostProcessing);
            }

            var isMethodResult = IsMethod(currentIndex, nodes);
            if (isMethodResult.Detected)
            {
                TryUpdatePreviousIdentifierToClassIfThatWasNamespace(currentIndex, nodes);
                return new ExtractedColourResult(NodeColors.Method, isMethodResult.SkipPostProcessing);
            }

            var isClassOrStructResult = IsClassOrStruct(currentIndex, nodes);
            if (isClassOrStructResult.Detected)
            {
                if (IsPopularStruct(node.Text))
                {
                    _FoundStructs.Add(node.Text);
                    return new ExtractedColourResult(NodeColors.Struct, isClassOrStructResult.SkipPostProcessing);
                }
                else if (IdentifierFirstCharCaseSeemsLikeVariable(node.Text))
                {
                    if (ThereIsThisInTheChainBefore(currentIndex, nodes))
                    {
                        return new ExtractedColourResult(NodeColors.Identifier, true);
                    }
                    else
                    {
                        // If Roslyn does not see it as a local variable because e.g declaration is not in the source code
                        // Then we have to decide whether this is local variable or property or field, etc.
                        // Then I think we should default to Property, but if it is top-level-statements like syntax,
                        // then I'd use LocalName because the result is better to read.
                        // Basically if we do not see declaration and it starts with lower case and is outside method body,
                        // then it should be Property due to popularity.

                        if (node.ClassificationType == ClassificationTypeNames.Identifier && _IsWithinMethod)
                            return new ExtractedColourResult(NodeColors.PropertyName, true);

                        return new ExtractedColourResult(NodeColors.LocalName);
                    }
                }
                else
                {
                    if (_FoundPropertiesOrFields.Contains(node.Text))
                    {
                        return new ExtractedColourResult(NodeColors.PropertyName);
                    }
                    else
                    {
                        AddClass(node.Text);
                        return new ExtractedColourResult(NodeColors.Class, isClassOrStructResult.SkipPostProcessing);
                    }
                }
            }


            var isPropertyOrFieldResult = IsPropertyOrField(currentIndex, nodes);
            if (isPropertyOrFieldResult.Detected)
            {
                if (IdentifierFirstCharCaseSeemsLikeVariable(node.Text))
                {
                    if (ThereIsThisInTheChainBefore(currentIndex, nodes) ||
                        (currentIndex > 0 && nodes[currentIndex - 1].Text == "."))
                    {
                        return new ExtractedColourResult(NodeColors.Identifier, true);
                    }
                    else
                    {
                        // If Roslyn does not see it as a local variable because e.g declaration is not in the source code
                        // Then we have to decide whether this is local variable or property or field, etc.
                        // I think we should default to Property, but if it is top-level-statements like syntax,
                        // then I'd use LocalName because the result is better to read.
                        // Basically if we do not see declaration and it starts with lower case and is outside method body,
                        // then it should be Property due to popularity.

                        if (node.ClassificationType == ClassificationTypeNames.Identifier && _IsWithinMethod)
                            return new ExtractedColourResult(NodeColors.PropertyName, true);

                        return new ExtractedColourResult(NodeColors.LocalName, isPropertyOrFieldResult.SkipPostProcessing);
                    }
                }
                else
                {
                    return new ExtractedColourResult(NodeColors.Identifier, true);
                }
            }
            else
            {
                if (currentIndex + 1 < nodes.Count &&
                    nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.Operator &&
                    nodes[currentIndex + 1].Text != "." &&
                    !_IsUsing)
                {
                    if (CheckIfChainIsMadeOfVariablesColors(currentIndex, _Output))
                    {
                        AddClass(node.Text);
                        return new ExtractedColourResult(NodeColors.Class);
                    }
                }

                if (_FoundPropertiesOrFields.Contains(node.Text))
                    return new ExtractedColourResult(NodeColors.PropertyName);

                return new ExtractedColourResult(NodeColors.Identifier);
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

            return new ExtractedColourResult(NodeColors.Keyword);
        }
        else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            if (node.Text == "{")
            {
                _BracketsCounter++;

                // void Test() {
                if (currentIndex > 2 && nodes[currentIndex - 1].Text == ")")
                {
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (i > 0 && nodes[i].Text == "(" && nodes[i - 1].ClassificationType == ClassificationTypeNames.MethodName)
                            _IsWithinMethod = true;
                    }
                }
            }

            if (node.Text == "}")
            {
                _BracketsCounter--;

                if (_BracketsCounter <= 0)
                    _IsWithinMethod = false;
            }

            if (node.Text == "(")
            {
                _ParenthesisCounter++;

                if (_IsNew && _ParenthesisCounter == 1)
                    _IsNew = false;
            }

            if (node.Text == ")")
            {
                _ParenthesisCounter--;

                if (_ParenthesisCounter <= 0)
                    _IsUsing = false;

                if (_ParenthesisCounter <= 0)
                    _IsNew = false;

                if (_ParenthesisCounter <= 0)
                    _IsTypeOf = false;
            }

            if (node.Text == ";")
            {
                _IsUsing = false;
                _IsNew = false;
                _IsTypeOf = false;
            }

            return new ExtractedColourResult(NodeColors.Punctuation);
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            return new ExtractedColourResult(NodeColors.Comment);
        }

        return new ExtractedColourResult(NodeColors.InternalError);
    }

    private DetectionStatus IsPropertyOrField(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var classifiers = new[]
        {
            ClassificationTypeNames.LocalName, ClassificationTypeNames.PropertyName, ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName, ClassificationTypeNames.ParameterName
        };

        if (ThereIsThisInTheChainBefore(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (!_IsNew && ThereIsMethodCallAhead(currentIndex, nodes) &&
            IdentifierFirstCharCaseSeemsLikeVariable(nodes[currentIndex].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (currentIndex >= 2 && nodes[currentIndex - 1].Text == "." && classifiers.Contains(nodes[currentIndex - 2].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoAhead && currentIndex >= 2
            && nodes[currentIndex + 1].ClassificationType == ClassificationTypeNames.Operator
            && nodes[currentIndex + 1].Text != "."
            && nodes[currentIndex - 1].Text == "("
            && nodes[currentIndex - 2].Text == "if")
        {
            AddVariable(node.Text);
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (IsOnInitializationList(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }

        return DetectionStatus.NotDetected();
    }

    private DetectionStatus IsInterface(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);
        var canGoTwoAhead = nodes.CanGoAhead(currentIndex, 2);

        var startsWithI = node.Text.StartsWith("I") && node.Text.Length > 1 && char.IsUpper(node.Text[1]);

        if (startsWithI && canGoBehind && new[] { ":", "<" }.Contains(nodes[currentIndex - 1].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoBehind && LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoAhead && new[] { ClassificationTypeNames.ParameterName, ClassificationTypeNames.FieldName }.Contains(nodes[currentIndex + 1].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoTwoAhead && nodes[currentIndex + 1].Text == ")" && nodes[currentIndex + 2].Text != "{")
        {
            if (currentIndex >= 2)
            {
                var suspectedId = nodes[currentIndex - 2].Id;
                var suspected = _Output.First(x => x.Id == suspectedId);

                if (nodes[currentIndex - 1].Text == "." && new[] { NodeColors.LocalName, NodeColors.ParameterName }.Contains(suspected.Colour))
                    return DetectionStatus.NotDetected();
            }

            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else
        {
            return DetectionStatus.NotDetected();
        }
    }

    private DetectionStatus IsMethod(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        // [InlineData("0001.txt")]
        // var a = list[test()];
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "[")
        {
            var identifier = nodes[currentIndex - 2].ClassificationType;

            var hasIdentifierBefore = identifier == ClassificationTypeNames.Identifier ||
                identifier == ClassificationTypeNames.LocalName;

            if (!hasIdentifierBefore)
                return DetectionStatus.NotDetected();
        }

        if (!_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            var availableKeywords = new List<string>(LanguageKeywords.AccessibilityModifiers)
            {
                "static",
                "virtual",
                "override"
            };

            if (currentIndex > 0 && availableKeywords.Contains(nodes[currentIndex - 1].Text))
                _IsWithinMethod = true;

            if (currentIndex > 1 && availableKeywords.Contains(nodes[currentIndex - 2].Text))
                _IsWithinMethod = true;

            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsUsing && !_IsUsing && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
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

            return validTypes.Contains(nodes[currentIndex + 2].ClassificationType) ?
                DetectionStatus.DetectedAndDontSkipPostProcessing() :
                DetectionStatus.NotDetected();
        }
        // db.NotifyHandler = OnNotify;
        else if (currentIndex > 0 && 
            currentIndex + 1 < nodes.Count &&
            nodes[currentIndex - 1].Text == "=" && nodes[currentIndex + 1].Text == ";" &&
            nodes[currentIndex].Text.StartsWith("On"))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else
        {
            return DetectionStatus.NotDetected();
        }
    }

    private DetectionStatus IsClassOrStruct(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var node = nodes[currentIndex];
        bool isPopularClassOrStruct = false;

        if (ThereIsThisInTheChainBefore(currentIndex, nodes))
        {
            return DetectionStatus.NotDetected();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == ":")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsNew && canGoAhead &&
            nodes[currentIndex + 1].ClassificationType == ClassificationTypeNames.Punctuation &&
            nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // be careful, if you remove those parenthesis around that assignment, then it'll change its behaviour
        else if ((isPopularClassOrStruct = IsPopularClass(node.Text) || IsPopularStruct(node.Text)) && !canGoBehind)
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (isPopularClassOrStruct && canGoBehind && nodes[currentIndex - 1].Text != ".")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "<")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "[")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // new DictionaryList<int, int>();
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // public void Test(Array<int> a)
        else if (canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikeParameter(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (RightSideOfAssignmentHasTheSameNameAfterNew(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (currentIndex >= 2 && nodes.Count >= 3 && nodes[currentIndex -1].ClassificationType == ClassificationTypeNames.Punctuation &&
            nodes[currentIndex - 1].Text == "(" && nodes[currentIndex - 2].Text == "foreach")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (IsParameterWithAttribute(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // cast
        else if (SeemsLikeCast(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && canGoAhead &&
            LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text) &&
            new[] { ClassificationTypeNames.FieldName, ClassificationTypeNames.ConstantName, ClassificationTypeNames.PropertyName }
            .Contains(nodes[currentIndex + 1].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && canGoAhead &&
            LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text) &&
            nodes[currentIndex + 1].Text == "[")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // WebResponse re;
        else if (nodes.Count > currentIndex + 2 &&
            new[] { ClassificationTypeNames.LocalName }.Contains(nodes[currentIndex + 1].ClassificationType) && 
            new[] { ";", "=" }.Contains(nodes[currentIndex + 2].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // typeof(Test)
        else if (canGoAhead && 
            currentIndex > 1 && 
            nodes[currentIndex + 1].Text == ")" &&
            nodes[currentIndex - 1].Text == "(" && 
            nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.Keyword &&
            nodes[currentIndex - 2].Text == "typeof")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }

        return DetectionStatus.NotDetected();
    }

    private bool SeemsLikeParameter(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var node = nodes[currentIndex];

        if (!canGoBehind || !canGoAhead)
            return false;

        if (!new[] { ",", "(" }.Contains(nodes[currentIndex - 1].Text))
            return false;

        if (nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.ParameterName && 
            nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.Punctuation)
            return false;

        if (currentIndex + 2 >= nodes.Count)
            return false;

        if (new[] { ",", ")" }.Contains(nodes[currentIndex + 2].Text))
            return true;

        if (nodes[currentIndex + 1].Text == "[" &&
            nodes[currentIndex + 2].Text == "]" &&
            new[] { ",", ")" }.Contains(nodes[currentIndex + 4].Text))
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
                return (i + 1 < nodes.Count && nodes[i + 1].Text == ".") ? false : true;
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

        if (currentIndex + 3 >= nodes.Count)
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
        return new string[] { ")", "=", ";", "}", ",", "&", 
                              "&&", "|", "||", "+", "-", "*", 
                              "/", "-=", "+=", "*=", "/=", "%=", 
                               "&=", "|=", "^=", ">>=", "<<="
                            }.Contains(next.Text);
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
        {
            return false;
        }

        return IsValidClassOrStructName(name.Text);
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

                if (nodes[i].ClassificationType == ClassificationTypeNames.Keyword && nodes[i].Text == "this")
                    return;

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
        if (s.Length > 0 && char.IsLower(s[0]))
            return true;

        if (s.Length > 0 && s[0] == '_')
            return true;

        return false;
    }

    private void AddClass(string s) => _FoundClasses.Add(s);
    private void AddVariable(string s) => _FoundPropertiesOrFields.Add(s);

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
}