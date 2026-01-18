using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private void HandleCounters()
    {
        if (CurrentText == "if" && TryPeekAhead(out var nextNode) && nextNode.Text == "(")
        {
            _ParenthesisCounter = 0;
            _InsideIfStatement = true;
        }

        if (CurrentText == "new")
        {
            _ParenthesisCounter = 0;
            _InsideNewStatement = true;
        }

        if (CurrentText == "(")
            _ParenthesisCounter++;

        if (CurrentText == ")" && _ParenthesisCounter > 0)
        {
            _ParenthesisCounter--;

            if (_ParenthesisCounter <= 0)
            {
                _InsideIfStatement = false;
                _InsideNewStatement = false;
            }
        }

        if (CurrentText == ";")
        {
            _ParenthesisCounter = 0;
            _InsideIfStatement = false;
            _InsideNewStatement = false;
        }
    }

    private bool HintsAndAlreadyClassifiedNodes(Node node)
    {
        if (_SimpleClassificationToColourMapper.TryGetValue(node.ClassificationType, out var simpleColour))
        {
            Logger.Info($"Matched: {simpleColour}");
            MarkNodeAs(node, simpleColour, true);
            return true;
        }

        if (_Hints.BuiltInTypes.Contains(node.Text))
        {
            MarkNodeAs(node, NodeColors.Keyword, true);
            return true;
        }

        return false;
    }

    private bool IsPunctuation()
    {
        if (CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            MarkNodeAs(NodeColors.Punctuation);
            return true;
        }

        return false;
    }

    private bool IsComment()
    {
        if (CurrentNode.ClassificationType.Contains("xml doc comment"))
        {
            MarkNodeAs(NodeColors.Comment);
            return true;
        }

        return false;
    }

    private bool IsKeyword()
    {
        if (CurrentNode.ClassificationType != ClassificationTypeNames.Keyword)
            return false;

        MarkNodeAs(NodeColors.Keyword);

        if (CurrentText == "using")
        {
            Logger.Info("Handling 'using'", 2);

            if (TryPeekAhead(out var peek2, 2) && peek2.Text == "=")
            {
                MoveNext();
                MarkNodeAs(NodeColors.Class);
                MoveNext();
                MarkNodeAs(ClassificationTypeNames.Operator);
            }
            else
            {
                // using Microsoft.AspNetCore.Mvc;
                var validIdentifiers = new[]
                {
                    ClassificationTypeNames.NamespaceName,
                    ClassificationTypeNames.Identifier,
                };

                const int STATE_IDENTIFIER = 0;
                const int STATE_DOT = 1;
                var currentState = STATE_IDENTIFIER;

                while (MoveNext())
                {
                    if (currentState == STATE_IDENTIFIER)
                    {
                        if (!CC.EqualsAnyOf(validIdentifiers))
                        {
                            MoveBehind();
                            break;
                        }

                        MarkNodeAs(NodeColors.Namespace, skipIdentifierPostProcess: true);
                        currentState = STATE_DOT;
                    }
                    else if (currentState == STATE_DOT)
                    {
                        if (!CC.EqualsAnyOf(ClassificationTypeNames.Punctuation, ClassificationTypeNames.Operator))
                        {
                            MoveBehind();
                            break;
                        }

                        if (CurrentText == ".")
                            MarkNodeAs(NodeColors.Operator, skipIdentifierPostProcess: true);

                        if (CurrentText == ";")
                            MarkNodeAs(NodeColors.Punctuation, skipIdentifierPostProcess: true);

                        currentState = STATE_IDENTIFIER;
                    }
                }
            }

            return true;
        }
        else if (CurrentText == "new")
        {
            Logger.Info("Handling 'new'", 2);

            if (MoveNext())
            {
                if (ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType))
                {
                    _InsideNewStatement = false;
                    return true;
                }
                else
                    MoveBehind();
            }
        }
        else if (CurrentText == "typeof")
        {
            Logger.Info("Handling 'typeof'", 2);
        }
        else if (CurrentText == "as")
        {
            Logger.Info("Handling 'as'", 2);
        }
        else if (CommonKeywordsBeforeTypeName.Contains(CurrentText))
        {
            Logger.Info("Handling CommonKeyword", 2);
            MarkNodeAs(NodeColors.Keyword);
        }

        return true;
    }

    private bool IsControl()
    {
        if (CurrentNode.ClassificationType != ClassificationTypeNames.ControlKeyword)
            return false;

        MarkNodeAs(NodeColors.Control);

        if (CurrentText == "if")
        {
            Logger.Info("Handling 'if'", 2);

            if (!MoveNext())
                return true;

            if (TryWalkIf())
                return true;
        }

        return true;
    }
}