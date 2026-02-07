using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal class HeuristicsGenerator
{
    private readonly Hints _Hints;

    public HeuristicsGenerator(Hints hints)
    {
        _Hints = hints;
    }

    public List<NodeAfterProcessing> Build(List<Node> input)
    {
        if (input == null || input.Count == 0)
            return new List<NodeAfterProcessing>();

        var chained = NodeChaining.ChainNodes(input, _Hints);
        var pm = PassManager.CreateDefault(_Hints);

        pm.RunPasses(chained);

        chained = NodeChaining.FlattenNodes(chained);
        AssignLineNumbers(chained);
        return MapInternalNodesToPublicType(chained);
    }

    private List<NodeAfterProcessing> MapInternalNodesToPublicType(List<Node> input)
    {
        return input.ConvertAll(x => new NodeAfterProcessing
        (
            x.Id,
            x.Colour,
            x.Text,
            x.Trivia,
            x.ClassificationType,
            x.UsesMostCommonColour,
            x.LineNumber,
            useHighlighting: false // it may be defined later by postprocessor
        ));
    }

    private void AssignLineNumbers(List<Node> output)
    {
        var currentLineNumber = 0;

        foreach (var node in output)
        {
            if (node.HasNewLine)
            {
                var newLinesCount = StringHelper.AllIndicesOf(node.Trivia, Environment.NewLine).Count;
                currentLineNumber += newLinesCount;
            }

            node.LineNumber = currentLineNumber;
        }
    }
}
