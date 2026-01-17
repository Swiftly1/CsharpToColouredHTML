using System.Diagnostics;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private void MarkNodeAs(Node node, string colour, bool skipIdentifierPostProcess = false)
    {
        var found = _Output.FirstOrDefault(x => x.Id == node.Id);

        if (found == null)
        {
            node.ModifyClassificationType(MapColourToClassificationType(colour, node.ClassificationType));
            _Output.Add(new NodeWithDetails
            (
                colour: colour,
                text: node.Text,
                trivia: node.Trivia,
                hasNewLine: node.HasNewLine,
                classificationType: MapColourToClassificationType(colour, node.ClassificationType),
                skipIdentifierPostProcessing: skipIdentifierPostProcess,
                id: node.Id
            ));
        }
        else
        {
            found.Colour = colour;
            found.ClassificationType = MapColourToClassificationType(colour, node.ClassificationType);
            found.SkipIdentifierPostProcessing = skipIdentifierPostProcess;
        }

        UpdateStats();
    }
    private void MarkNodeAs(string colour, bool skipIdentifierPostProcess = false)
    {
        MarkNodeAs(CurrentNode, colour, skipIdentifierPostProcess);
    }

    [DebuggerStepThrough]
    private void UpdateStats()
    {
        if (_Output.Count == 0)
            return;

        var latest = _Output.Last();

        if (latest.Colour == NodeColors.Class)
            _FoundClasses.Add(latest.Text);

        if (latest.Colour == NodeColors.Struct)
            _FoundStructs.Add(latest.Text);
    }

    [DebuggerStepThrough]
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < _OriginalNodes.Count;
    }

    [DebuggerStepThrough]
    public bool CanMoveAhead(int jumpSize=1)
    {
        if (_OriginalNodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex + jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < _OriginalNodes.Count;
    }

    [DebuggerStepThrough]
    public bool MoveNext(int jumpSize=1)
    {
        if (CanMoveAhead(jumpSize))
        {
            _CurrentIndex += jumpSize;
            return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public bool MoveBehind(int jumpSize = 1)
    {
        if (CanMoveBehind(jumpSize))
        {
            _CurrentIndex -= jumpSize;
            return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public bool TryPeekAtIndex(out Node nodeAfterMove, int index)
    {
        nodeAfterMove = null!;

        if (_OriginalNodes is null)
            return false;

        var isOk = index >= 0 && index < _OriginalNodes.Count;

        if (isOk)
            nodeAfterMove = _OriginalNodes[index];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool TryPeekAhead(out Node nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (_OriginalNodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex + jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < _OriginalNodes.Count;

        if (isOk)
            nodeAfterMove = _OriginalNodes[adjustedIndex];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool CanMoveBehind(int jumpSize=1)
    {
        if (_OriginalNodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex - jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < _OriginalNodes.Count;
    }

    [DebuggerStepThrough]
    public bool TryPeekBehind(out Node nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (_OriginalNodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex - jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < _OriginalNodes.Count;

        if (isOk)
            nodeAfterMove = _OriginalNodes[adjustedIndex];

        return isOk;
    }

    private static bool NameLikeInterface(string text)
    {
        return text.StartsWith("I") && text.Length > 1 && char.IsUpper(text[1]);
    }

    private bool IsValidClassOrStructName(string text, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.Skip(1).All(x => char.IsLetter(x) || char.IsNumber(x) || x == '_');
    }

    private bool IsPopularEnum(string text)
    {
        return _Hints.ReallyPopularEnums.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
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

    private static readonly List<string> CommonKeywordsBeforeTypeName = new List<string>
    {
        "public", "private", "internal", "sealed", "protected", "readonly", "static", "override", "event", "required",
        "virtual", "unsafe", "partial", "delegate"
    };

    private static readonly List<string> AccessibilityModifiers = new List<string>
    {
        "public", "private", "protected", "internal", "protected internal", "private protected"
    };

    private static readonly List<string> Operators = new List<string>
    {
        "+", "-", "/", "*", "=", "==", "+=", "-=", "*=", "/=", "!=", "&",
        "^", "|", "&&", "||", "??", "%=", "|=", "^=", "<<=", ">>=", "??=",
        ">>>", ">>>=", "<", ">", "is", "as", ">="
    };

    private bool SoundsLikeEventOrHandler(string name)
    {
        return name.StartsWith("On") || name.EndsWith("Event") || name.EndsWith("Handler") || name.EndsWith("Changed");
    }

    private string MapColourToClassificationType(string colour, string defaultClassification)
    {
        return colour switch
        {
            NodeColors.Numeric => ClassificationTypeNames.NumericLiteral,
            NodeColors.Method => ClassificationTypeNames.MethodName,
            NodeColors.Class => ClassificationTypeNames.ClassName,
            NodeColors.Keyword => ClassificationTypeNames.Keyword,
            NodeColors.String => ClassificationTypeNames.StringLiteral,
            NodeColors.Control => ClassificationTypeNames.ControlKeyword,
            NodeColors.Interface => ClassificationTypeNames.InterfaceName,
            NodeColors.Comment => ClassificationTypeNames.Comment,
            NodeColors.Preprocessor => ClassificationTypeNames.PreprocessorKeyword,
            NodeColors.PreprocessorText => ClassificationTypeNames.PreprocessorText,
            NodeColors.Struct => ClassificationTypeNames.StructName,
            NodeColors.Namespace => ClassificationTypeNames.NamespaceName,
            NodeColors.EnumMemberName => ClassificationTypeNames.EnumMemberName,
            NodeColors.EnumName => ClassificationTypeNames.EnumName,
            NodeColors.Identifier => ClassificationTypeNames.Identifier,
            NodeColors.Operator => ClassificationTypeNames.Operator,
            NodeColors.PropertyName => ClassificationTypeNames.PropertyName,
            NodeColors.FieldName => ClassificationTypeNames.FieldName,
            NodeColors.LabelName => ClassificationTypeNames.LabelName,
            NodeColors.OperatorOverloaded => ClassificationTypeNames.OperatorOverloaded,
            NodeColors.ConstantName => ClassificationTypeNames.ConstantName,
            NodeColors.ParameterName => ClassificationTypeNames.ParameterName,
            NodeColors.LocalName => ClassificationTypeNames.LocalName,
            NodeColors.ExtensionMethodName => ClassificationTypeNames.ExtensionMethodName,
            NodeColors.TypeParameterName => ClassificationTypeNames.TypeParameterName,
            NodeColors.RecordStructName => ClassificationTypeNames.RecordStructName,
            NodeColors.NumericLiteral => ClassificationTypeNames.NumericLiteral,
            NodeColors.Delegate => ClassificationTypeNames.DelegateName,
            NodeColors.EventName => ClassificationTypeNames.EventName,
            NodeColors.ExcludedCode => ClassificationTypeNames.ExcludedCode,
            _ => defaultClassification
        };
    }
}