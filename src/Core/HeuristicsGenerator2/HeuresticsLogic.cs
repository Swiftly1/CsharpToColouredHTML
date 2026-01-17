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

    private bool HintsAndAlreadyClassifiedNodes()
    {
        Logger.Info("Hints & Already Classified");
        if (_SimpleClassificationToColourMapper.TryGetValue(CurrentNode.ClassificationType, out var simpleColour))
        {
            MarkNodeAs(simpleColour);
            return true;
        }

        if (_Hints.BuiltInTypes.Contains(CurrentNode.Text))
        {
            MarkNodeAs(NodeColors.Keyword);
            return true;
        }

        return false;
    }

    private bool IsPunctuation()
    {
        Logger.Info("Is Punctuation");
        if (CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            MarkNodeAs(NodeColors.Punctuation);
            return true;
        }

        return false;
    }

    private bool IsComment()
    {
        Logger.Info("Is Comment");

        if (CurrentNode.ClassificationType.Contains("xml doc comment"))
        {
            MarkNodeAs(NodeColors.Comment);
            return true;
        }

        return false;
    }

    private bool IsKeyword()
    {
        Logger.Info("Is Keyword");

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
        }
        else if (CurrentText == "new")
        {
            Logger.Info("Handling 'new'", 2);

            if (MoveNext())
            {
                if (ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType))
                    return true;
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
}