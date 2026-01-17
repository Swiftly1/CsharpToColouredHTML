using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator2
{
    private void GenerateHeuristics()
    {
        var printEndLoopMessages = true;

        do
        {
            try
            {
                if (!printEndLoopMessages)
                {
                    Logger.Info($"END of Loop");
                    Logger.Info($"");
                }

                printEndLoopMessages = false;
                Logger.PrintCurrentText(CurrentText, _CurrentIndex);

                HandleCounters();

                if (HintsAndAlreadyClassifiedNodes())
                {
                    Logger.Success("Hints & Already Classified");
                    continue;
                }

                if (IsKeyword())
                {
                    Logger.Success("Is Keyword");
                    continue;
                }

                if (IsControl())
                {
                    Logger.Success("Is Control");
                    continue;
                }

                if (IsComment())
                {
                    Logger.Success("Is Comment");
                    continue;
                }

                if (IsOperator())
                {
                    Logger.Success("Is Operator");
                    continue;
                }

                if (IsPunctuation())
                {
                    Logger.Success("Is Punctuation");
                    continue;
                }

                if (IsMethod())
                {
                    Logger.Success("Is Method");
                    continue;
                }

                if (IsExpression())
                {
                    Logger.Success("Is Expression");
                    continue;
                }

                if (IsType())
                {
                    Logger.Success("Is Type");
                    continue;
                }

                MarkNodeAs(NodeColors.DefaultColour);
            }
            catch
            {
                MarkNodeAs(NodeColors.InternalError);
            }
        } while (MoveNext());
    }

    private bool IsOperator()
    {
        if (CurrentNode.ClassificationType == ClassificationTypeNames.Operator)
        {
            if (CurrentText != "=")
            {
                MarkNodeAs(NodeColors.Operator);
                return true;
            }
        }

        return false;
    }

    private bool IsExpression()
    {
        if (TryWalkBeforeAssignmentExpression())
            return true;

        if (TryWalkAssignmentExpression())
            return true;

        return false;
    }

    private bool IsMethod()
    {
        if (_InsideNewStatement)
            return false;

        // constructor
        // public HomeController()
        if (TryPeekBehind(out var modifier) && AccessibilityModifiers.Contains(modifier.Text) &&
            TryPeekAhead(out var ahead) && ahead.Text == "(")
        {
            MarkNodeAs(ResolveClassOrStructName(CurrentNode), true);
            return true;
        }

        return TryWalkMethod();
    }

    private bool IsType()
    {
        return ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.Default);
    }
}