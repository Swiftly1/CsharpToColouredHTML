using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

public enum ExpressionWalkState
{
    Chain,
    DotOrEnd,
    Operator,
    GenericsOpening,
}

public enum ExpressionWalkMode
{
    Default
}

internal partial class NodeEnumerationHelper
{
    public bool ConsumeExpressionAhead(ExpressionWalkState initialState, ExpressionWalkMode mode, bool markIt = true)
    {
        Logger.Info($"ConsumeExpressionAhead '{CurrentText}'", 3);
        return false;
    }

    public string ResolveVariable(Node node, bool hint_IsClass = false)
    {
        var checkResult = IsAlreadyClassifiedExpression(node);

        if (checkResult.Success)
            return checkResult.Value;

        var text = node.Text;

        if (Context.FoundLocalNames.Contains(text))
            return NodeColors.LocalName;

        if (Context.FoundProperties.Contains(text))
            return NodeColors.PropertyName;

        if (Context.FoundFields.Contains(text))
            return NodeColors.FieldName;

        if (text.FirstCharIsUpper())
        {
            if (hint_IsClass)
            {
                return ResolveClassOrStructName(node);
            }
            return NodeColors.PropertyName;
        }

        if (text.StartsWith("_"))
            return NodeColors.PropertyName;

        return NodeColors.LocalName;
    }

    private (bool Success, string Value) IsAlreadyClassifiedExpression(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.LocalName)
            return (Success: true, Value: NodeColors.LocalName);

        if (node.ClassificationType == ClassificationTypeNames.FieldName)
            return (Success: true, Value: NodeColors.FieldName);

        if (node.ClassificationType == ClassificationTypeNames.PropertyName)
            return (Success: true, Value: NodeColors.PropertyName);

        if (node.ClassificationType == ClassificationTypeNames.ConstantName)
            return (Success: true, Value: NodeColors.ConstantName);

        if (node.ClassificationType == ClassificationTypeNames.NumericLiteral)
            return (Success: true, Value: NodeColors.NumericLiteral);

        if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
            return (Success: true, Value: NodeColors.String);

        return (Success: false, Value: string.Empty);
    }

    public string FieldOrProperty(Node node)
    {
        var result = IsAlreadyClassifiedExpression(node);

        if (result.Success)
            return result.Value;

        return NodeColors.PropertyName;
    }

    private readonly string[] _validExpressionNameClassifications =
    [
        ClassificationTypeNames.NamespaceName,
        ClassificationTypeNames.LocalName,
        ClassificationTypeNames.FieldName,
        ClassificationTypeNames.PropertyName,
        ClassificationTypeNames.Identifier,
        ClassificationTypeNames.ConstantName,
        ClassificationTypeNames.ParameterName,
        ClassificationTypeNames.MethodName,
    ];

    public bool ExpressionHasValidIdentifier(NodeWrapper node)
    {
        if (_validExpressionNameClassifications.Contains(node.ClassificationType))
            return true;

        if (node.Text.EqualsAnyOf("out", "var", "ref", "this", "null"))
            return true;

        if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            return Context.Hints.BuiltInTypes.Contains(node.Text);
        }

        return false;
    }
}
