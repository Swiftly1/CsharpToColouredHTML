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

internal partial class ExpressionWalker
{
    //public bool ConsumeExpressionAhead(ExpressionWalkState initialState, ExpressionWalkMode mode, bool markIt = true)
    //{
    //    Logger.Info($"ConsumeExpressionAhead '{CurrentText}'", 3);
    //    return false;
    //}


    //private readonly string[] _validExpressionNameClassifications =
    //[
    //    ClassificationTypeNames.NamespaceName,
    //    ClassificationTypeNames.LocalName,
    //    ClassificationTypeNames.FieldName,
    //    ClassificationTypeNames.PropertyName,
    //    ClassificationTypeNames.Identifier,
    //    ClassificationTypeNames.ConstantName,
    //    ClassificationTypeNames.ParameterName,
    //    ClassificationTypeNames.MethodName,
    //];

    //public bool ExpressionHasValidIdentifier(Node node)
    //{
    //    if (_validExpressionNameClassifications.Contains(node.ClassificationType))
    //        return true;

    //    if (node.Text.EqualsAnyOf("out", "var", "ref", "this", "null"))
    //        return true;

    //    if (node.ClassificationType == ClassificationTypeNames.Keyword)
    //    {
    //        return Walker.Hints.BuiltInTypes.Contains(node.Text);
    //    }

    //    return false;
    //}
}
