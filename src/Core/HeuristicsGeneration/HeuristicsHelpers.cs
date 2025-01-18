using System.Diagnostics;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private void MarkNodeAs(string colour, bool skipIdentifierPostProcess = false)
    {
        var found = _Output.FirstOrDefault(x => x.Id == CurrentNode.Id);

        if (found == null)
        {
            CurrentNode.ModifyClassificationType(MapColourToClassificationType(colour, CurrentNode.ClassificationType));
            _Output.Add(new NodeWithDetails
            (
                colour: colour,
                text: CurrentText,
                trivia: CurrentNode.Trivia,
                hasNewLine: CurrentNode.HasNewLine,
                parenthesisCounter: _ParenthesisCounter,
                classificationType: MapColourToClassificationType(colour, CurrentNode.ClassificationType),
                skipIdentifierPostProcessing: skipIdentifierPostProcess,
                id: CurrentNode.Id)
            );
        }
        else
        {
            found.Colour = colour;
            found.ClassificationType = MapColourToClassificationType(colour, CurrentNode.ClassificationType);
        }

        UpdateStats();
    }

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
                parenthesisCounter: _ParenthesisCounter,
                classificationType: MapColourToClassificationType(colour, node.ClassificationType),
                skipIdentifierPostProcessing: skipIdentifierPostProcess,
                id: node.Id
            ));
        }
        else
        {
            found.Colour = colour;
            found.ClassificationType = MapColourToClassificationType(colour, node.ClassificationType);
        }
        UpdateStats();
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

    private bool IdentifierFirstCharCaseSeemsLikeVariable(string s)
    {
        if (s.Length > 0 && char.IsLower(s[0]))
            return true;

        if (s.Length > 0 && s[0] == '_')
            return true;

        return false;
    }

    private bool IsClassOrStructAlreadyFound(string text)
    {
        if (IsPopularStruct(text))
            return true;

        if (IsPopularClass(text))
            return true;

        if (_FoundClasses.Contains(text))
            return true;

        if (_FoundStructs.Contains(text))
            return true;

        return false;
    }

    private string ResolveName(string text)
    {
        if (!IsValidClassOrStructName(text))
            return NodeColors.Identifier;

        if (IsPopularStruct(text))
            return NodeColors.Struct;

        if (IsPopularClass(text))
            return NodeColors.Class;

        if (IsPopularEnum(text) || text.EndsWith("Enum"))
            return NodeColors.EnumName;

        if (_FoundClasses.Contains(text))
            return NodeColors.Class;

        if (_FoundStructs.Contains(text))
            return NodeColors.Struct;

        if (_FoundPropertiesOrFields.Contains(text))
            return NodeColors.PropertyName;

        if (_FoundLocalNames.Contains(text))
            return NodeColors.LocalName;

        return NodeColors.Class;
    }

    private string ResolveClassOrStructName(string text)
    {
        if (!IsValidClassOrStructName(text))
            return NodeColors.Identifier;

        if (IsPopularStruct(text))
            return NodeColors.Struct;

        if (IsPopularClass(text))
            return NodeColors.Class;

        if (_FoundClasses.Contains(text))
            return NodeColors.Class;

        if (_FoundStructs.Contains(text))
            return NodeColors.Struct;

        return NodeColors.Class;
    }

    private bool IsValidClassOrStructName(string text)
    {
        if (IdentifierFirstCharCaseSeemsLikeVariable(text))
            return false;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.Skip(1).All(x => char.IsLetter(x) || char.IsNumber(x));
    }

    private bool IsPopularEnum(string text)
    {
        if (IdentifierFirstCharCaseSeemsLikeVariable(text))
            return false;

        return _Hints.ReallyPopularEnums.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPopularClass(string text)
    {
        if (IdentifierFirstCharCaseSeemsLikeVariable(text))
            return false;

        return _Hints.ReallyPopularClasses.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase))
            ||
            _Hints.ReallyPopularClassSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPopularStruct(string text)
    {
        if (IdentifierFirstCharCaseSeemsLikeVariable(text))
            return false;

        return _Hints.ReallyPopularStructs.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase))
            ||
            _Hints.ReallyPopularStructsSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    public static readonly List<string> AccessibilityModifiers = new List<string>
    {
        "public", "private", "internal", "sealed", "protected", "readonly", "static"
    };

    public static readonly List<string> Operators = new List<string>
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
            NodeColors.Class => ClassificationTypeNames.ClassName,
            NodeColors.PropertyName => ClassificationTypeNames.PropertyName,
            NodeColors.LocalName => ClassificationTypeNames.LocalName,
            NodeColors.FieldName => ClassificationTypeNames.FieldName,
            NodeColors.Keyword => ClassificationTypeNames.Keyword,
            NodeColors.Struct => ClassificationTypeNames.StructName,
            NodeColors.Method => ClassificationTypeNames.MethodName,
            NodeColors.Interface => ClassificationTypeNames.InterfaceName,
            NodeColors.ConstantName => ClassificationTypeNames.ConstantName,
            NodeColors.ParameterName => ClassificationTypeNames.ParameterName,
            NodeColors.TypeParameterName => ClassificationTypeNames.TypeParameterName,
            _ => defaultClassification
        };
    }
}