using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
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

internal class TypeWalker
{
    public NodeEnumerationHelper Walker { get; }

    public SharedPassContext Context { get; }

    public TypeWalker(NodeEnumerationHelper walker, SharedPassContext context)
    {
        Walker = walker;
        Context = context;
    }

    public bool ConsumeTypeAhead(TypeWalkState initialState, TypeWalkMode mode)
    {
        Logger.Info($"ConsumeTypeAhead '{Walker.CurrentText}'", 3);

        if (!TypeHasValidIdentifier(Walker.CurrentNode))
            return false;

        var foundColours = new List<(Node Node, string Colour)>();

        var previousStates = new List<TypeWalkState>();
        var currentState = initialState;

        int genericsOpeningCounter = 0;

        if (initialState == TypeWalkState.GenericsName)
            genericsOpeningCounter = 1;

        do
        {
            if (Walker.CurrentText == ";")
            {
                foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                break;
            }

            if (initialState == TypeWalkState.GenericsName && genericsOpeningCounter <= 0)
            {
                Walker.MoveBehind();
                break;
            }

            previousStates.Add(currentState);

            if (currentState == TypeWalkState.TypeName)
            {
                if (Walker.CC.EqualsAnyOf(_validTypeNameClassifications) || Walker.CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (Walker.TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((Walker.CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }

                    currentState = TypeWalkState.TypeNameDotOrEnd;
                }
                else if (Walker.CurrentText == "(")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == TypeWalkState.TypeNameDotOrEnd)
            {
                if (Walker.CurrentText == ".")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Operator));
                    currentState = TypeWalkState.TypeName;
                }
                else if (Walker.CurrentText == "<")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else if (Walker.CurrentText == "?")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    break;
                }
                else if (Walker.CurrentText == "(")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    break;
                }
                else
                {
                    Walker.MoveBehind();
                    break;
                }
            }
            else if (currentState == TypeWalkState.TupleName)
            {
                if (Walker.CC.EqualsAnyOf(_validTypeNameClassifications) || Walker.CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (Walker.TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((Walker.CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }

                    currentState = TypeWalkState.TupleDotOrEnd;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == TypeWalkState.TupleDotOrEnd)
            {
                if (Walker.CurrentText == ",")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else if (Walker.CurrentText == ")")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    if (genericsOpeningCounter > 0)
                    {
                        currentState = TypeWalkState.GenericsDotOrEnd;
                    }
                    else
                    {
                        currentState = TypeWalkState.TypeNameDotOrEnd;
                    }
                }
                else if (Walker.CurrentText == "<")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else if (Walker.CC == ClassificationTypeNames.Identifier)
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.PropertyName));
                    currentState = TypeWalkState.TupleDotOrEnd;
                }
                else
                {
                    Walker.MoveBehind();
                    break;
                }
            }
            else if (currentState == TypeWalkState.GenericsName)
            {
                if (Walker.CC.EqualsAnyOf(_validTypeNameClassifications) || Walker.CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (Walker.TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((Walker.CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((Walker.CurrentNode, Context.NameResolver.ResolveClassOrStructName(Walker.CurrentNode)));
                    }

                    currentState = TypeWalkState.GenericsDotOrEnd;
                }
                else if (Walker.CurrentText == "(")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == TypeWalkState.GenericsDotOrEnd)
            {
                if (Walker.CurrentText == ".")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Operator));
                    currentState = TypeWalkState.GenericsName;
                }
                else if (Walker.CurrentText == ",")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.GenericsName;
                }
                else if (Walker.CurrentText == "<")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else if (Walker.CurrentText == ">")
                {
                    foundColours.Add((Walker.CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter--;
                    if (genericsOpeningCounter > 0)
                    {
                        currentState = TypeWalkState.GenericsDotOrEnd;
                    }
                    else
                    {
                        currentState = TypeWalkState.TypeNameDotOrEnd;
                    }
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

    private readonly string[] _validTypeNameClassifications =
    [
        ClassificationTypeNames.Identifier,
        ClassificationTypeNames.NamespaceName,
        ClassificationTypeNames.ClassName,
        ClassificationTypeNames.StructName,
        ClassificationTypeNames.RecordClassName,
        ClassificationTypeNames.RecordStructName,
        ClassificationTypeNames.InterfaceName,
        ClassificationTypeNames.TypeParameterName,
        ClassificationTypeNames.LocalName,
    ];

    public bool TypeHasValidIdentifier(Node node)
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
