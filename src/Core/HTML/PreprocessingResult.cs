namespace CsharpToColouredHTML.Core;

internal record PreprocessingResult
{
    public PreprocessingResult(List<NodeWithDetails> nodes, string mostCommonColour)
    {
        Nodes = nodes;
        MostCommonColour = mostCommonColour;
    }

    public List<NodeWithDetails> Nodes { get; init; }

    public string MostCommonColour { get; init; }
}
