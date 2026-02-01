using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

public enum TypeWalkState
{
    TypeName,
    TypeNameDotOrEnd,

    GenericsName,
    GenericsDotOrEnd,

    TupleName,
    TupleDotOrEnd,
}

public enum TypeWalkMode
{
    MustBeType,
    Default
}

internal partial class NodeEnumerationHelper
{
    public bool ConsumeTypeAhead(TypeWalkState initialState, TypeWalkMode mode)
    {
        Logger.Info($"ConsumeTypeAhead '{CurrentText}'", 3);
        return false;
    }

    public string ResolveUnkownName(Node node)
    {
        var result = CheckIfLooksLikeVariable(node);

        if (result.IsVariable)
            return result.Value;

        var checkResult = IsAlreadyClassifiedExpression(node);

        if (checkResult.Success)
            return checkResult.Value;

        return ResolveClassOrStructName(node);
    }

    public (bool IsVariable, string Value) CheckIfLooksLikeVariable(Node node)
    {
        if (node.Text.StartsWith("_"))
            return (IsVariable: true, Value: ResolveVariable(node));

        if (!node.Text.FirstCharIsUpper())
            return (IsVariable: true, Value: ResolveVariable(node));

        return (IsVariable: false, Value: string.Empty);
    }

    public string ResolveClassOrStructName(Node node)
    {
        var checkResult = IsAlreadyClassOrStruct(node);

        if (checkResult.Success)
            return checkResult.Value;

        var text = node.Text;

        if (Context.IsPopularStruct(text))
            return NodeColors.Struct;

        if (Context.IsPopularClass(text))
            return NodeColors.Class;

        if (Context.IsPopularEnum(text) || text.EndsWith("Enum"))
            return NodeColors.EnumName;

        if (Context.FoundClasses.Contains(text))
            return NodeColors.Class;

        if (Context.FoundStructs.Contains(text))
            return NodeColors.Struct;

        if (PassHelpers.NameLikeInterface(text))
            return NodeColors.Interface;

        if (Context.Hints.BuiltInTypes.Contains(text))
            return NodeColors.Keyword;

        return NodeColors.Class;
    }

    private static (bool Success, string Value) IsAlreadyClassOrStruct(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.StructName)
            return (Success: true, Value: NodeColors.Struct);

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
            return (Success: true, Value: NodeColors.Class);

        if (node.ClassificationType == ClassificationTypeNames.InterfaceName)
            return (Success: true, Value: NodeColors.Interface);

        if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
            return (Success: true, Value: NodeColors.RecordStructName);

        if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
            return (Success: true, Value: NodeColors.Class);

        return (Success: false, Value: string.Empty);
    }

    private readonly string[] _validTypeNameClassifications =
    [
        ClassificationTypeNames.Identifier,
        ClassificationTypeNames.NamespaceName,
        ClassificationTypeNames.ClassName,
        ClassificationTypeNames.StructName,
        ClassificationTypeNames.RecordClassName,
        ClassificationTypeNames.RecordStructName,
        ClassificationTypeNames.InterfaceName,
        ClassificationTypeNames.TypeParameterName
    ];

    public bool TypeHasValidIdentifier(NodeWrapper node)
    {
        if (_validTypeNameClassifications.Contains(node.ClassificationType))
            return true;

        if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            return Context.Hints.BuiltInTypes.Contains(node.Text);
        }

        return false;
    }
}
