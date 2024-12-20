using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
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
            return HandleIdentifier(currentIndex, nodes);
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
            return HandlePunctuation(currentIndex, nodes);
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            return new ExtractedColourResult(NodeColors.Comment);
        }

        return new ExtractedColourResult(NodeColors.InternalError);
    }
}