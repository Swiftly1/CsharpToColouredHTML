using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

public enum ExpressionWalkState
{
    Chain,
    DotOrEnd,
    Operator
}

public enum ExpressionWalkMode
{
    Default
}

internal partial class NodeEnumerationHelper
{
    public bool ConsumeExpressionAhhead(ExpressionWalkState initialState, ExpressionWalkMode mode)
    {
        Logger.Info($"ConsumeExpressionAhead '{CurrentText}'", 3);

        if (!ExpressionHasValidIdentifier(this.CurrentNode))
            return false;

        var foundColours = new List<(NodeInternalRepresentation Node, string Colour)>();

        var previousStates = new List<ExpressionWalkState>();
        var currentState = initialState;

        do
        {
            if (CurrentText == ";")
            {
                foundColours.Add((CurrentNode, NodeColors.Punctuation));
                break;
            }

            if (currentState == ExpressionWalkState.Chain)
            {
                if (!ExpressionHasValidIdentifier(CurrentNode))
                    break;


                currentState = ExpressionWalkState.DotOrEnd;

                if (TryPeekAhead(out var peekedAhead))
                {
                    if (peekedAhead.Text == "(")
                        foundColours.Add((CurrentNode, NodeColors.Method));
                    else if (peekedAhead.Text == ".")
                        foundColours.Add((CurrentNode, ResolveExpressionElement(CurrentNode)));
                    else if (peekedAhead.ClassificationType == ClassificationTypeNames.Operator)
                    {
                        foundColours.Add((CurrentNode, ResolveExpressionElement(CurrentNode)));
                        currentState = ExpressionWalkState.Operator;
                    }
                    else
                    {
                        foundColours.Add((CurrentNode, ResolveExpressionElement(CurrentNode)));
                        break;
                    }
                }
                else
                {
                    foundColours.Add((CurrentNode, ResolveExpressionElement(CurrentNode)));
                }

            }
            else if (currentState == ExpressionWalkState.DotOrEnd)
            {
                if (CurrentText == ".")
                {
                    foundColours.Add((CurrentNode, NodeColors.Operator));
                    currentState = ExpressionWalkState.Chain;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == ExpressionWalkState.Operator)
            {
                if (CC == ClassificationTypeNames.Operator)
                {
                    foundColours.Add((CurrentNode, NodeColors.Operator));
                    currentState = ExpressionWalkState.Chain;
                }
                else
                {
                    MoveBehind();
                    break;
                }
            }
            else
            {
                throw new NotImplementedException("State is not handled.");
            }

            previousStates.Add(currentState);
        } while (MoveNext());

        foreach (var entry in foundColours)
        {
            MarkNodeAs(entry.Node, entry.Colour);
        }

        return foundColours.Any();
    }

    public string ResolveExpressionElement(NodeInternalRepresentation node)
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
            return NodeColors.PropertyName;

        return NodeColors.Default;
    }

    private static (bool Success, string Value) IsAlreadyClassifiedExpression(NodeInternalRepresentation node)
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

    private readonly string[] _validExpressionNameClassifications =
    [
        ClassificationTypeNames.NamespaceName,
        ClassificationTypeNames.LocalName,
        ClassificationTypeNames.FieldName,
        ClassificationTypeNames.PropertyName,
        ClassificationTypeNames.Identifier,
        ClassificationTypeNames.ConstantName,
    ];

    public bool ExpressionHasValidIdentifier(NodeInternalRepresentation node)
    {
        if (_validExpressionNameClassifications.Contains(node.ClassificationType))
            return true;

        if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            return Context.Hints.BuiltInTypes.Contains(node.Text);
        }

        return false;
    }
}
