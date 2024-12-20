using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.Emitters;

public interface IEmitter
{
    public string Emit(List<NodeAfterProcessing> nodes);
}