namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal static class ListUtils
{
    public static bool CanGoAhead<T>(this List<T> list, int currentIndex, int jumpSize = 1)
    {
        if (list is null)
            return false;

        if (currentIndex < 0)
            return false;

        var adjustedIndex = currentIndex + jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < list.Count;
    }

    public static bool CanGoBehind<T>(this List<T> list, int currentIndex, int jumpSize = 1)
    {
        if (list is null)
            return false;

        var adjustedIndex = currentIndex - jumpSize;
        return adjustedIndex >= 0 && adjustedIndex < list.Count;
    }
}
