using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    public enum TypeWalkState
    {
        TypeName,
        TypeNameDotOrEnd,

        GenericsName,
        GenericsDotOrEnd,

        TupleName,
        TupleDotOrEnd
    }

    public bool ConsumeTypeAhead(TypeWalkState initialState)
    {
        Logger.Info($"ConsumeTypeAhead '{CurrentText}'", 3);

        if (!TypeHasValidIdentifier(CurrentNode))
            return false;

        var foundColours = new List<(Node Node, string Colour)>();

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
                if (CC.EqualsAnyOf(_validTypeNameClassifications))
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
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
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
                else
                {
                    MoveBehind();
                    break;
                }
            }
            else if (currentState == TypeWalkState.TupleName)
            {
                if (CC.EqualsAnyOf(_validTypeNameClassifications))
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
                if (CC.EqualsAnyOf(_validTypeNameClassifications))
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
                    foundColours.Add((CurrentNode, NodeColors.Punctuation));
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

            var fod = foundColours.FirstOrDefault(x => x.Node == CurrentNode);
            if (fod == default)
            {
                foundColours.Add((CurrentNode, NodeColors.DefaultColour));
            }
        } while (MoveNext());

        foreach (var entry in foundColours)
        {
            MarkNodeAs(entry.Node, entry.Colour);
        }

        return foundColours.Any();
    }

    private string ResolveClassOrStructName(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.StructName)
            return NodeColors.Struct;

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
            return NodeColors.Class;

        if (node.ClassificationType == ClassificationTypeNames.InterfaceName)
            return NodeColors.Interface;

        if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
            return NodeColors.RecordStructName;

        if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
            return NodeColors.Class;

        var text = node.Text;

        if (IsPopularStruct(text))
            return NodeColors.Struct;

        if (IsPopularClass(text))
            return NodeColors.Class;

        if (IsPopularEnum(text) || text.EndsWith("Enum"))
            return NodeColors.EnumName;

        if (_FoundClasses.Contains(text))
            return NodeColors.Class;

        if (_FoundStructs.Contains(text))
            return NodeColors.Struct;

        if (NameLikeInterface(text))
            return NodeColors.Interface;

        if (_Hints.BuiltInTypes.Contains(text))
            return NodeColors.Keyword;

        return NodeColors.Class;
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

    public bool TypeHasValidIdentifier(Node node)
    {
        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.RecordClassName,
            ClassificationTypeNames.RecordStructName,
            ClassificationTypeNames.InterfaceName,
            ClassificationTypeNames.TypeParameterName
        };

        if (valid_identifiers.Contains(node.ClassificationType))
            return true;

        if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            return _Hints.BuiltInTypes.Contains(node.Text);
        }

        return false;
    }
}
