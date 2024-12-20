using CsharpToColouredHTML.Core.Nodes;

public class CsharpColourerSettings
{
    public Action<List<NodeAfterProcessing>>? PostProcessingAction { get; set; }
}