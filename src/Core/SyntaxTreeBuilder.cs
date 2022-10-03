using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

internal class HeuristicsGenerator
{
    private bool _IsUsing = false;

    private bool _IsNew = false;

    private int _ParenthesisCounter = 0;

    private readonly Hints _Hints;

    private List<NodeWithDetails> _alreadyProcessed = new List<NodeWithDetails>();

    private List<string> _FoundClasses = new List<string>();
    private List<string> _FoundStructs = new List<string>();
    private List<string> _FoundInterfaces = new List<string>();

    public HeuristicsGenerator(Hints hints)
    {
        _Hints = hints;
    }

    public List<NodeWithDetails> Build(List<Node> input)
    {
        Reset();
        return Preprocess(input);
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

    private List<NodeWithDetails> Preprocess(List<Node> nodes)
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
            _alreadyProcessed.Add(nodeWithDetails);
        }

        PostProcess(_alreadyProcessed);

        return _alreadyProcessed;
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

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
        {
            colour = NodeColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Comment)
        {
            colour = NodeColors.Comment;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PreprocessorKeyword)
        {
            colour = NodeColors.Preprocessor;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PreprocessorText)
        {
            colour = NodeColors.PreprocessorText;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StructName)
        {
            colour = NodeColors.Struct;
        }
        else if (node.ClassificationType == ClassificationTypeNames.InterfaceName)
        {
            colour = NodeColors.Interface;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
        {
            colour = NodeColors.Namespace;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumName)
        {
            colour = NodeColors.EnumName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumMemberName)
        {
            colour = NodeColors.EnumMemberName;
        }
        else if (_Hints.BuiltInTypes.Contains(node.Text))
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
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            colour = NodeColors.String;
        }
        else if (node.ClassificationType == ClassificationTypeNames.VerbatimStringLiteral)
        {
            colour = NodeColors.String;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LocalName)
        {
            colour = NodeColors.LocalName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.MethodName)
        {
            colour = NodeColors.Method;
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
        else if (node.ClassificationType == ClassificationTypeNames.Operator)
        {
            colour = NodeColors.Operator;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PropertyName)
        {
            colour = NodeColors.PropertyName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ParameterName)
        {
            colour = NodeColors.ParameterName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.FieldName)
        {
            colour = NodeColors.FieldName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NumericLiteral)
        {
            colour = NodeColors.NumericLiteral;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            colour = NodeColors.Control;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LabelName)
        {
            colour = NodeColors.LabelName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.OperatorOverloaded)
        {
            colour = NodeColors.OperatorOverloaded;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
        {
            colour = NodeColors.RecordStructName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
        {
            colour = NodeColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.TypeParameterName)
        {
            colour = NodeColors.TypeParameterName;
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            colour = NodeColors.Comment;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ExtensionMethodName)
        {
            colour = NodeColors.ExtensionMethodName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ConstantName)
        {
            colour = NodeColors.ConstantName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.DelegateName)
        {
            colour = NodeColors.Delegate;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EventName)
        {
            colour = NodeColors.EventName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ExcludedCode)
        {
            colour = NodeColors.ExcludedCode;
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
                var suspected = _alreadyProcessed.FirstOrDefault(x => x.Id == suspectedId);
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

        var alreadyProcessedSuspectedNode = _alreadyProcessed.LastOrDefault(x => x.Id == suspectedNode.Id);

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