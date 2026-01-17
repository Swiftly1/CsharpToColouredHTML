using CsharpToColouredHTML.Core.Nodes;

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
                Logger.Info($"Current Text: '{CurrentText}'");

                HandleCounters();

                if (HintsAndAlreadyClassifiedNodes())
                    continue;

                if (IsKeyword())
                    continue;

                if (IsComment())
                    continue;

                if (IsPunctuation())
                    continue;

                if (IsType())
                    continue;

                MarkNodeAs(NodeColors.DefaultColour);
            }
            catch
            {
                MarkNodeAs(NodeColors.InternalError);
            }
        } while (MoveNext());
    }

    private bool IsType()
    {
        if (ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.Default))
            return true;

        return false;
    }
}