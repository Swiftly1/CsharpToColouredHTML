using CsharpToColouredHTML.Core.Miscs;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class SharedPassContext
{
    public Hints Hints = new Hints();

    public HashSet<string> FoundClasses = new();
    public HashSet<string> FoundStructs = new();
    public HashSet<string> FoundInterfaces = new();
    public HashSet<string> FoundPropertiesOrFields = new();
    public HashSet<string> FoundLocalNames = new();

    public HashSet<string> FoundNamespaceParts = new();
    public HashSet<string> FoundNamespaces = new();
}