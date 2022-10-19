using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

internal class HeuristicsGenerator
{
    private bool _IsUsing = false;

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
        Reset();
        ProcessData(input);
        PostProcess(_Output);
        return _Output;
    }

    private void Reset()
    {
        _FoundClasses.Clear();
        _FoundInterfaces.Clear();
        _FoundStructs.Clear();
        _IsUsing = false;
        _IsNew = false;
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
        var identifiers = alreadyProcessed.Where(x => x.Colour == NodeColors.Identifier).ToList();

        foreach (var entry in identifiers)
        {
            if (_FoundClasses.Contains(entry.Text))
                entry.Colour = NodeColors.Class;

            if (_FoundInterfaces.Contains(entry.Text))
                entry.Colour = NodeColors.Interface;

            if (_FoundStructs.Contains(entry.Text))
                entry.Colour = NodeColors.Struct;
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
            else if (IsClass(currentIndex, nodes))
            {
                if (node.Text.Length > 0 && char.IsLower(node.Text[0]))
                {
                    colour = NodeColors.LocalName;
                }
                else
                {
                    colour = NodeColors.Class;
                    _FoundClasses.Add(node.Text);
                }
            }
            else if (IsStruct(currentIndex, nodes))
            {
                colour = NodeColors.Struct;
                _FoundStructs.Add(node.Text);
            }
            else
            {
                colour = NodeColors.Identifier;
            }
        }
        else if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            if (node.Text == "using")
                _IsUsing = true;

            if (node.Text == "new")
                _IsNew = true;

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
            }

            if (node.Text == ";")
            {
                _IsUsing = false;
                _IsNew = false;
            }

            colour = NodeColors.Punctuation;
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            colour = NodeColors.Comment;
        }

        return colour;
    }

    private bool IsStruct(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var canGoBehind = currentIndex > 0;

        var isPopularStruct = IsPopularStruct(node.Text);

        if (isPopularStruct && !canGoBehind)
        {
            return true;
        }

        if (isPopularStruct && canGoBehind && nodes[currentIndex - 1].Text != ".")
        {
            return true;
        }

        return false;
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
                var suspected = _Output.FirstOrDefault(x => x.Id == suspectedId);
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
        else
        {
            return false;
        }
    }

    private bool IsClass(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        var node = nodes[currentIndex];
        bool isPopularClass = false;

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
        else if ((isPopularClass = IsPopularClass(node.Text)) && !canGoBehind)
        {
            return true;
        }
        else if (isPopularClass && canGoBehind && nodes[currentIndex - 1].Text != ".")
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
        else if (canGoAhead && nodes[currentIndex + 1].Text == "<" && !IsPopularStruct(node.Text))
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
        else
        {
            return false;
        }
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
                return true;
            }
        }

        return false;
    }

    private bool SeemsLikePropertyUsage(int currentIndex, List<Node> nodes)
    {
        if (_IsUsing)
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

        // OLEMSGICON.OLEMSGICON_WARNING,
        return new string[] { ")", "(", "=", ";", "}", ",", "&", "&&", "|", "||" }.Contains(next.Text);
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
        // there's method call ahead so I guess that's an class, orrr namespace :(

        var i = currentIndex;
        var state = 0;

        while (++i < nodes.Count)
        {
            var current = nodes[i];

            if (state == 0 && current.ClassificationType == ClassificationTypeNames.Operator)
            {
                state = 1;
            }
            else if (state == 1 && current.ClassificationType == ClassificationTypeNames.Identifier)
            {
                state = 0;
            }
            else if (current.Text == "(")
            {
                return true;
            }
            else
            {
                return false;
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

        var alreadyProcessedSuspectedNode = _Output.LastOrDefault(x => x.Id == suspectedNode.Id);

        if (alreadyProcessedSuspectedNode == null)
            return;

        if (alreadyProcessedSuspectedNode.Colour == NodeColors.Identifier)
        {
            // 0 = currently at Identifier, expecting Operator
            // 1 = currently at Operator, expecting Identifier

            var state = 0;
            var theChainIsMadeOfIdentifiers = true;

            var validTypes = new[]
            {
                ClassificationTypeNames.LocalName,
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.ConstantName,
                ClassificationTypeNames.ParameterName,
                ClassificationTypeNames.Operator
            };

            for (int i = currentIndex - 3; i >= 0; i--)
            {
                if (!validTypes.Contains(nodes[i].ClassificationType))
                {
                    if (nodes[i].ClassificationType == ClassificationTypeNames.Punctuation && nodes[i].Text == ">")
                        theChainIsMadeOfIdentifiers = false;

                    break;
                }

                if (nodes[i].ClassificationType == ClassificationTypeNames.Operator && nodes[i].Text != ".")
                    break;

                if (state == 0)
                {
                    if (nodes[i].ClassificationType == ClassificationTypeNames.Operator)
                    {
                        state = 1;
                        continue;
                    }
                    else
                    {
                        theChainIsMadeOfIdentifiers = false;
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
                        theChainIsMadeOfIdentifiers = false;
                    }
                }
            }

            if (theChainIsMadeOfIdentifiers)
            {
                alreadyProcessedSuspectedNode.Colour = NodeColors.Class;
            }
        }
    }
}