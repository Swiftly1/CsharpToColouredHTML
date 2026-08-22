using System.Diagnostics;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class EnumerationHelper<T>(List<T> nodes) where T : class
{
    public int CurrentIndex = 0;

    public T CurrentNode => Nodes[CurrentIndex];

    public List<T> Nodes { get; } = nodes;

    [DebuggerStepThrough]
    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool CanMoveAhead(int jumpSize = 1)
    {
        if (Nodes is null)
            return false;

        if (CurrentIndex < 0)
            return false;

        var adjustedIndex = CurrentIndex + jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool MoveNext(int jumpSize = 1)
    {
        if (CanMoveAhead(jumpSize))
        {
            CurrentIndex += jumpSize;
            return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public bool MoveBehind(int jumpSize = 1)
    {
        if (CanMoveBehind(jumpSize))
        {
            CurrentIndex -= jumpSize;
            return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public bool TryPeekAtIndex(out T nodeAfterMove, int index)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        var isOk = index >= 0 && index < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[index];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool TryPeekAhead(out T nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        if (CurrentIndex < 0)
            return false;

        var adjustedIndex = CurrentIndex + jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[adjustedIndex];

        return isOk;
    }

    [DebuggerStepThrough]
    public bool CanMoveBehind(int jumpSize = 1)
    {
        if (Nodes is null)
            return false;

        if (CurrentIndex < 0)
            return false;

        var adjustedIndex = CurrentIndex - jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < Nodes.Count;
    }

    [DebuggerStepThrough]
    public bool TryPeekBehind(out T nodeAfterMove, int jumpSize = 1)
    {
        nodeAfterMove = null!;

        if (Nodes is null)
            return false;

        if (CurrentIndex < 0)
            return false;

        var adjustedIndex = CurrentIndex - jumpSize;
        var isOk = adjustedIndex >= 0 && adjustedIndex < Nodes.Count;

        if (isOk)
            nodeAfterMove = Nodes[adjustedIndex];

        return isOk;
    }

    public void Reset()
    {
        CurrentIndex = 0;
    }
}
