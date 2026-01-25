using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class SharedPassContext
{
    public Hints Hints = new Hints();

    public HashSet<string> FoundClasses = new();
    public HashSet<string> FoundStructs = new();
    public HashSet<string> FoundInterfaces = new();
    public HashSet<string> FoundProperties = new();
    public HashSet<string> FoundFields = new();
    public HashSet<string> FoundLocalNames = new();

    public HashSet<string> FoundNamespaceParts = new();
    public HashSet<string> FoundNamespaces = new();

    public HashSet<(string FunctionName, int Index)> FunctionLocations = new();

    public bool IsPopularEnum(string text)
    {
        return Hints.ReallyPopularEnums.Any(x => string.Equals(x, text));
    }

    public bool IsPopularClass(string text)
    {
        return Hints.ReallyPopularClasses.Any(x => string.Equals(x, text))
            ||
            Hints.ReallyPopularClassSubstrings.Any(x => text.Contains(x));
    }

    public bool IsPopularStruct(string text)
    {
        return Hints.ReallyPopularStructs.Any(x => string.Equals(x, text))
            ||
            Hints.ReallyPopularStructsSubstrings.Any(x => text.Contains(x));
    }
}