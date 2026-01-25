using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class SharedPassContext
{
    public Hints Hints = new();

    public HashSet<string> FoundClasses = [];
    public HashSet<string> FoundStructs = [];
    public HashSet<string> FoundInterfaces = [];
    public HashSet<string> FoundProperties = [];
    public HashSet<string> FoundFields = [];
    public HashSet<string> FoundLocalNames = [];

    public HashSet<string> FoundNamespaceParts = [];
    public HashSet<string> FoundNamespaces = [];

    public HashSet<(string FunctionName, int Index)> FunctionLocations = [];

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