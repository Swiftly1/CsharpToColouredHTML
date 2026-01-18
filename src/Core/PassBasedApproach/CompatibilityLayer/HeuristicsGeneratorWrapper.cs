using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.CompatibilityLayer;

internal class HeuristicsGeneratorWrapper
{
    private readonly Hints _Hints;

    public HeuristicsGeneratorWrapper(Hints hints)
    {
        _Hints = hints;
    }

    public List<NodeAfterProcessing> Build(List<Node> input)
    {
        if (input == null || input.Count == 0)
            return new List<NodeAfterProcessing>();

        var remapped = MapNodesIntoInternalRepresentation(input);
        var pm = PassManager.CreateDefault(_Hints);

        pm.RunPasses(remapped);

        AssignLineNumbers(remapped);
        return MapOutputToPublicType(remapped);
    }

    private List<NodeInternalRepresentation> MapNodesIntoInternalRepresentation(List<Node> input)
    {
        var output = new List<NodeInternalRepresentation>();

        foreach (var node in input)
        {
            output.Add(new NodeInternalRepresentation
            (
                colour: NodeColors.Default,
                text: node.Text,
                trivia: node.Trivia,
                hasNewLine: node.HasNewLine,
                classificationType: node.ClassificationType,
                skipIdentifierPostProcessing: false,
                id: node.Id
            ));
        }

        return output;
    }

    internal List<NodeAfterProcessing> MapOutputToPublicType(List<NodeInternalRepresentation> input)
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


    private void AssignLineNumbers(List<NodeInternalRepresentation> output)
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
