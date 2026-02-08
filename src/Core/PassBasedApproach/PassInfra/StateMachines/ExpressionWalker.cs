using System;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
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

internal class ExpressionWalker
{
    public NodeEnumerationHelper Walker { get; }

    public SharedPassContext Context { get; }

    public ExpressionWalker(NodeEnumerationHelper walker, SharedPassContext context)
    {
        Walker = walker;
        Context = context;
    }


    public bool ConsumeExpressionAhead(ExpressionWalkState initialState, ExpressionWalkMode mode, bool markIt = true)
    {
        Logger.Info($"ConsumeExpressionAhead '{Walker.CurrentText}'", 3);

        if (!ExpressionHasValidIdentifier(Walker.CurrentNode))
            return false;

        var foundColours = new List<(Node Node, string Colour)>();

        var previousStates = new List<ExpressionWalkState>();
        var currentState = initialState;
        var isRootFound = false;

        do
        {
            if (Walker.CurrentText == ";")
            {
                foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                break;
            }

            previousStates.Add(currentState);

            if (currentState == ExpressionWalkState.Chain)
            {
                if (!isRootFound)
                {
                    if (Walker.CC == ClassificationTypeNames.LocalName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.LocalName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.FieldName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.FieldName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.PropertyName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.PropertyName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.ParameterName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.ParameterName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.Identifier)
                    {
                        if (Walker.TryPeekAhead(out var next))
                        {
                            if (next.Text == "(")
                            {
                                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Method);
                                Context.MarkNodeAs(next, NodeColors.Punctuation);
                            }
                            else if (next.Text == ".")
                            {
                                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.PropertyName);
                                Context.MarkNodeAs(next, NodeColors.Operator);
                            }
                        }
                        else
                        {
                            Context.NameResolver.ResolveVariable(Walker.CurrentNode);
                        }
                        isRootFound = true;
                        currentState = ExpressionWalkState.DotOrEnd;
                    }
                    continue;
                }
                else
                {
                    if (Walker.CC == ClassificationTypeNames.FieldName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.FieldName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.PropertyName)
                    {
                        Context.MarkNodeAs(Walker.CurrentNode, NodeColors.PropertyName);
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    if (Walker.CC == ClassificationTypeNames.Identifier)
                    {
                        if (Walker.TryPeekAhead(out var next))
                        {
                            if (next.Text == "(")
                            {
                                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Method);
                                Context.MarkNodeAs(next, NodeColors.Punctuation);
                            }
                        }
                        else
                        {
                            Context.MarkNodeAs(Walker.CurrentNode, NodeColors.PropertyName);
                        }
                        currentState = ExpressionWalkState.DotOrEnd;
                    }
                }
            }
            else if (currentState == ExpressionWalkState.DotOrEnd)
            {
                if (Walker.CurrentText == ".")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Operator));
                    currentState = ExpressionWalkState.Chain;
                }
                else
                {
                    Walker.MoveBehind();
                    break;
                }
            }
            else
            {
                throw new NotImplementedException("State is not handled.");
            }
        } while (Walker.MoveNext());

        foreach (var entry in foundColours)
        {
            Context.MarkNodeAs(entry.Node, entry.Colour);
        }

        return foundColours.Any();
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

    public bool ExpressionHasValidIdentifier(Node node)
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
