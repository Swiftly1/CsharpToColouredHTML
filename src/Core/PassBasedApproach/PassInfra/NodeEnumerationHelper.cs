using System.Diagnostics;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class NodeEnumerationHelper
{
    private int _CurrentIndex = 0;

    public NodeInternalRepresentation CurrentNode => Nodes[_CurrentIndex];

    public string CurrentText => Nodes[_CurrentIndex].Text;

    // Current Classification - "CC" in short because it is used very often.
    public string CC => Nodes[_CurrentIndex].ClassificationType;

    public List<NodeInternalRepresentation> Nodes { get; }

    public SharedPassContext Context { get; }

    public NodeEnumerationHelper(List<NodeInternalRepresentation> nodes, SharedPassContext ctx)
    {
        Nodes = nodes;
        Context = ctx;
    }

    public void MarkNodeAs(NodeInternalRepresentation node, string colour, bool skipIdentifierPostProcess = false)
    {
        var found = Nodes.FirstOrDefault(x => x.Id == node.Id);

        if (found == null)
            return;

        if (!found.SkipIdentifierPostProcessing)
        {
            found.Colour = colour;
            found.ClassificationType = MapColourToClassificationType(colour, node.ClassificationType);
            found.SkipIdentifierPostProcessing = skipIdentifierPostProcess;
        }

        UpdateStats();
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

    [DebuggerStepThrough]
    private void UpdateStats()
    {
        if (Nodes.Count == 0)
            return;

        var latest = Nodes.Last();

        if (latest.Colour == NodeColors.Class)
            Context.FoundClasses.Add(latest.Text);

        if (latest.Colour == NodeColors.Struct)
            Context.FoundStructs.Add(latest.Text);
    }

    [DebuggerStepThrough]
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool CanMoveAhead(int jumpSize = 1)
    {
        if (Nodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex + jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool MoveNext(int jumpSize = 1)
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
    public bool TryPeekAtIndex(out NodeInternalRepresentation nodeAfterMove, int index)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        var isOk = index >= 0 && index < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[index];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool TryPeekAhead(out NodeInternalRepresentation nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex + jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[adjustedIndex];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool CanMoveBehind(int jumpSize = 1)
    {
        if (Nodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex - jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool TryPeekBehind(out NodeInternalRepresentation nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        if (_CurrentIndex < 0)
            return false;

        var adjustedIndex = _CurrentIndex - jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[adjustedIndex];

        return isOk;
    }
}
