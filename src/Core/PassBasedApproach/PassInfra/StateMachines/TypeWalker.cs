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
    TupleDotOrEnd
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

        if (!TypeHasValidIdentifier(this.CurrentNode))
            return false;

        var foundColours = new List<(NodeInternalRepresentation Node, string Colour)>();

        var previousStates = new List<TypeWalkState>();
        var currentState = initialState;

        int genericsOpeningCounter = 0;

        do
        {
            if (CurrentText == ";")
            {
                foundColours.Add((CurrentNode, NodeColors.Punctuation));
                break;
            }

            previousStates.Add(currentState);

            if (currentState == TypeWalkState.TypeName)
            {
                if (CC.EqualsAnyOf(_validTypeNameClassifications) || CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
                    }

                    currentState = TypeWalkState.TypeNameDotOrEnd;
                }
                else if (CurrentText == "(")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == TypeWalkState.TypeNameDotOrEnd)
            {
                if (CurrentText == ".")
                {
                    foundColours.Add((CurrentNode, NodeColors.Operator));
                    currentState = TypeWalkState.TypeName;
                }
                else if (CurrentText == "<")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else if (CurrentText == "?")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    break;
                }
                else if (CurrentText == "(")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    break;
                }
                else
                {
                    MoveBehind();
                    break;
                }
            }
            else if (currentState == TypeWalkState.TupleName)
            {
                if (CC.EqualsAnyOf(_validTypeNameClassifications) || CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
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
                if (CurrentText == ",")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else if (CurrentText == ")")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
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
                else if (CurrentText == "<")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else
                {
                    MoveBehind();
                    break;
                }
            }
            else if (currentState == TypeWalkState.GenericsName)
            {
                if (CC.EqualsAnyOf(_validTypeNameClassifications) || CurrentText.EqualsAnyOf(Context.Hints.BuiltInTypes.ToArray()))
                {
                    if (TryPeekAhead(out var peekedAhead))
                    {
                        if (peekedAhead.Text == ".")
                            foundColours.Add((CurrentNode, NodeColors.Namespace));
                        else
                            foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
                    }
                    else
                    {
                        foundColours.Add((CurrentNode, ResolveClassOrStructName(CurrentNode)));
                    }

                    currentState = TypeWalkState.GenericsDotOrEnd;
                }
                else if (CurrentText == "(")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.TupleName;
                }
                else
                {
                    break;
                }
            }
            else if (currentState == TypeWalkState.GenericsDotOrEnd)
            {
                if (CurrentText == ".")
                {
                    foundColours.Add((CurrentNode, NodeColors.Operator));
                    currentState = TypeWalkState.GenericsName;
                }
                else if (CurrentText == ",")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    currentState = TypeWalkState.GenericsName;
                }
                else if (CurrentText == "<")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter++;
                    currentState = TypeWalkState.GenericsName;
                }
                else if (CurrentText == ">")
                {
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
                    genericsOpeningCounter--;
                    currentState = TypeWalkState.GenericsDotOrEnd;
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
        } while (MoveNext());

        foreach (var entry in foundColours)
        {
            MarkNodeAs(entry.Node, entry.Colour);
        }

        return foundColours.Any();
    }

    private string ResolveClassOrStructName(NodeInternalRepresentation node)
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

    private static (bool Success, string Value) IsAlreadyClassOrStruct(NodeInternalRepresentation node)
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

    public bool TypeHasValidIdentifier(NodeInternalRepresentation node)
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
