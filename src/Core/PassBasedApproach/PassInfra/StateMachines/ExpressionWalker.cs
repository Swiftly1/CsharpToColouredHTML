using System;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

public enum ExpressionWalkState
{
    Chain,
    DotOrEnd,
    Operator,
    Generics,
}

public enum ExpressionWalkMode
{
    Default,
    FromTheMiddle,
    DelegateRegistrationUnregistration
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
        var parenthesisCounter = 0;

        do
        {
            if (Walker.CurrentText == ";")
            {
                foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                Walker.MoveBehind();
                break;
            }

            if (Walker.CurrentText == "(")
                parenthesisCounter++;

            if (Walker.CurrentText == ")")
                parenthesisCounter--;

            var insideFunctionCall = parenthesisCounter > 0;

            previousStates.Add(currentState);

            if (currentState == ExpressionWalkState.Chain)
            {
                if (!isRootFound)
                {
                    if (Walker.CC == ClassificationTypeNames.LocalName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.LocalName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.FieldName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.FieldName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.PropertyName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.PropertyName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.ParameterName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.PropertyName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.StringLiteral)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.String));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.Keyword)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.Keyword));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = false;
                    }
                    else if (Walker.CC == ClassificationTypeNames.Identifier)
                    {
                        isRootFound = true;
                        currentState = ExpressionWalkState.DotOrEnd;
                        if (Walker.TryPeekAhead(out var next))
                        {
                            if (next.Text == "(")
                            {
                                foundColours.Add((Walker.CurrentNode, NodeColors.Method));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else if (next.Text == ".")
                            {
                                var colour = string.Empty;

                                if (Walker.TryPeekAhead(out var next2, 2))
                                {
                                    if (next2.ClassificationType == ClassificationTypeNames.MethodName)
                                    {
                                        colour = Context.NameResolver.ResolveUnkownName(Walker.CurrentNode);
                                    }
                                    else if (next2.ClassificationType.EqualsAnyOf(
                                        ClassificationTypeNames.ClassName,
                                        ClassificationTypeNames.RecordClassName,
                                        ClassificationTypeNames.RecordStructName,
                                        ClassificationTypeNames.StructName))
                                    {
                                        colour = NodeColors.Namespace;
                                    }
                                    else if (mode == ExpressionWalkMode.Default)
                                    {
                                        colour = Context.NameResolver.ResolveUnkownName(Walker.CurrentNode);
                                    }
                                    else
                                    {
                                        colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);
                                    }
                                }
                                else
                                {
                                    if (mode == ExpressionWalkMode.Default)
                                    {
                                        colour = Context.NameResolver.ResolveUnkownName(Walker.CurrentNode);
                                    }
                                    else
                                    {
                                        colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);
                                    }
                                }

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else if (next.Text == ",")
                            {
                                var colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else if (next.Text == "}")
                            {
                                var colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else if (next.Text == "<" && next.ClassificationType == ClassificationTypeNames.Punctuation)
                            {
                                var colour = Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode);

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.Generics;
                            }
                            else if (next.Text == ";")
                            {
                                var colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);

                                if (mode == ExpressionWalkMode.DelegateRegistrationUnregistration && foundColours.Count == 0)
                                {
                                    if (NameResolver.SoundsLikeEventOrHandler(Walker.CurrentText))
                                    {
                                        colour = NodeColors.Method;
                                    }
                                }

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else if (next.ClassificationType == ClassificationTypeNames.Operator)
                            {
                                var colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else
                            {
                                var colour = string.Empty;

                                if (mode == ExpressionWalkMode.Default)
                                {
                                    colour = Context.NameResolver.ResolveUnkownName(Walker.CurrentNode);
                                }
                                else
                                {
                                    colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);
                                }

                                foundColours.Add((Walker.CurrentNode, colour));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                        }
                        else
                        {
                            foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveVariable(Walker.CurrentNode)));
                            currentState = ExpressionWalkState.Chain;
                        }
                    }
                }
                else
                {
                    if (Walker.CC == ClassificationTypeNames.FieldName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.FieldName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.PropertyName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.PropertyName));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    else if (Walker.CC == ClassificationTypeNames.MethodName)
                    {
                        foundColours.Add((Walker.CurrentNode, NodeColors.Method));
                        currentState = ExpressionWalkState.DotOrEnd;
                        isRootFound = true;
                    }
                    if (Walker.CC == ClassificationTypeNames.Identifier)
                    {
                        if (Walker.TryPeekAhead(out var next))
                        {
                            if (next.Text == "(")
                            {
                                foundColours.Add((Walker.CurrentNode, NodeColors.Method));
                                currentState = ExpressionWalkState.DotOrEnd;
                            }
                            else
                            {
                                var colour = string.Empty;

                                colour = Context.NameResolver.ResolveVariable(Walker.CurrentNode);

                                foundColours.Add((Walker.CurrentNode, colour));
                            }
                        }
                        else
                        {
                            foundColours.Add((Walker.CurrentNode, NodeColors.PropertyName));
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
                else if (Walker.CurrentText == "," && insideFunctionCall)
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Chain;
                    isRootFound = false;
                }
                else if (Walker.CurrentText == "(")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Chain;
                    isRootFound = false;
                }
                else if (Walker.CurrentText == "<")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.Chain;
                }
                else if (Walker.CurrentText == ">")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = ExpressionWalkState.DotOrEnd;
                }
                else
                {
                    Walker.MoveBehind();
                    break;
                }
            }
            else if (currentState == ExpressionWalkState.Generics)
            {
                if (Walker.CurrentText == "<")
                    Walker.MoveNext();
                var typeWalker = new TypeWalker(Walker, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.GenericsName, TypeWalkMode.MustBeType);
                currentState = ExpressionWalkState.DotOrEnd;
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
