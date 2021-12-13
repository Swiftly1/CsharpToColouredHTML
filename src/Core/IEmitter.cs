namespace CsharpToColouredHTML.Core;

public interface IEmitter
{
    public string Text { get; }

    public void Emit(List<Node> nodes);
}