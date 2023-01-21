namespace CsharpToColouredHTML.Core;

public interface IEmitter
{
    public string Emit(List<NodeAfterProcessing> nodes);
}