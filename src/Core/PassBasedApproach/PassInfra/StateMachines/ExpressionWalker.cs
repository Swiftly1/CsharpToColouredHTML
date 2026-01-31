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
                    {
                        foundColours.Add((CurrentNode, NodeColors.Method));
                    }
                    else if (peekedAhead.Text == "<")
                    {
                        foundColours.Add((CurrentNode, ResolveVariable(CurrentNode, true)));
                        currentState = ExpressionWalkState.GenericsOpening;
                    }
                    else if (peekedAhead.Text == ".")
                    {
                        if (foundColours.Count >= 2 && foundColours[^1].Node.Text == ".")
                        {
                            if (foundColours[^2].Colour == NodeColors.Class)
                            {
                                foundColours.Add((CurrentNode, ResolveVariable(CurrentNode, false)));
                            }
                        }
                        else
                        {
                            foundColours.Add((CurrentNode, ResolveVariable(CurrentNode, true)));
                        }
                        currentState = ExpressionWalkState.Operator;
                    }
                    else if (peekedAhead.ClassificationType == ClassificationTypeNames.Operator)
                    {
                        foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));
                        currentState = ExpressionWalkState.Operator;
                    }
                    else if (peekedAhead.Text == ",")
                    {
                        foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));
                        currentState = ExpressionWalkState.DotOrEnd;
                    }
                    else if (peekedAhead.Text == ")")
                    {
                        foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));
                        currentState = ExpressionWalkState.DotOrEnd;
                    }
                    else if (peekedAhead.Text.EqualsAnyOf("out", "var", "ref"))
                    {
                        if (CurrentNode.ClassificationType == ClassificationTypeNames.Keyword)
                            foundColours.Add((CurrentNode, NodeColors.Keyword));
                        else
                            foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));

                        currentState = ExpressionWalkState.Chain;
                    }
                    else
                    {
                        foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));
                        break;
                    }
                }
                else
                {
                    foundColours.Add((CurrentNode, ResolveVariable(CurrentNode)));
                }

            }
            else if (currentState == ExpressionWalkState.DotOrEnd)
            {
                if (CurrentText == ".")
                {
                    foundColours.Add((CurrentNode, NodeColors.Operator));
                    currentState = ExpressionWalkState.Chain;
                }
                else if (CurrentText == "(")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Chain;
                }
                else if (CurrentText == ")")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Operator;
                }
                else if (CurrentText == ",")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
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
                else if (CurrentText == ",")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Chain;
                }
                else
                {
                    MoveBehind();
                    break;
                }
            }
            else if (currentState == ExpressionWalkState.GenericsOpening)
            {
                if (!MoveNext())
                    break;

                if (ConsumeTypeAhead(TypeWalkState.GenericsName, TypeWalkMode.MustBeType))
                {
                    currentState = ExpressionWalkState.DotOrEnd;
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

        if (markIt)
        {
            foreach (var entry in foundColours)
            {
                MarkNodeAs(entry.Node, entry.Colour);
            }
        }

        return foundColours.Any();
    }

    public string ResolveVariable(NodeInternalRepresentation node, bool hint_IsClass = false)
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

    private (bool Success, string Value) IsAlreadyClassifiedExpression(NodeInternalRepresentation node)
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

    public string FieldOrProperty(NodeInternalRepresentation node)
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
    ];

    public bool ExpressionHasValidIdentifier(NodeInternalRepresentation node)
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
