using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private int _ParenthesisCounter = 0;

    private bool _InsideIfStatement = true;
    private bool _InsideNewStatement = true;

    private readonly Hints _Hints;

    private int _CurrentIndex = 0;
    private Node CurrentNode => _OriginalNodes[_CurrentIndex];
    private string CurrentText => _OriginalNodes[_CurrentIndex].Text;
    // Current Classification - "CC" in short because it is used very often.
    private string CC => _OriginalNodes[_CurrentIndex].ClassificationType;

    internal List<Node> _OriginalNodes = new();

    private List<NodeWithDetails> _Output = new();

    private HashSet<string> _FoundClasses = new();
    private HashSet<string> _FoundStructs = new();
    private HashSet<string> _FoundInterfaces = new();
    private HashSet<string> _FoundPropertiesOrFields = new();
    private HashSet<string> _FoundLocalNames = new();

    private HashSet<string> _FoundNamespaceParts = new();
    private HashSet<string> _FoundNamespaces = new();

    private Dictionary<string, string> _SimpleClassificationToColourMapper { get; } = new()
    {
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

    public HeuristicsGenerator2(Hints hints)
    {
        _Hints = hints;
    }

    public List<NodeAfterProcessing> Build(List<Node> input)
    {
        Reset();

        if (input == null || input.Count == 0)
            return new List<NodeAfterProcessing>();

        _OriginalNodes = input;
        GenerateHeuristics();

        //if (_OriginalNodes.Count != _Output.Count)
        //    throw new Exception("Something was lost");

        AssignLineNumbers(_Output);
        return MapOutputToPublicType();
    }

    internal List<NodeAfterProcessing> MapOutputToPublicType()
    {
        return _Output.ConvertAll(x => new NodeAfterProcessing
        (
            x.Id,
            x.Colour,
            x.Text,
            x.Trivia,
            x.ClassificationType,
            x.UsesMostCommonColour,
            x.LineNumber,
            useHighlighting: false // it may be defined later by postprocessor
        ));
    }

    public void Reset()
    {
        _FoundClasses.Clear();
        _FoundInterfaces.Clear();
        _FoundStructs.Clear();
        _FoundPropertiesOrFields.Clear();
        _FoundLocalNames.Clear();
        _FoundNamespaces.Clear();
        _FoundNamespaceParts.Clear();
        _OriginalNodes.Clear();
        _Output.Clear();
        _ParenthesisCounter = 0;
        _CurrentIndex = 0;
        _InsideIfStatement = false;
        _InsideNewStatement = false;
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
}