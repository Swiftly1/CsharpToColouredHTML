using System;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;

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

    public HashSet<(string FunctionName, int Index)> FunctionDeclarationLocations = [];

    //new List<string> { ClassificationTypeNames.StringEscapeCharacter };

    public HashSet<(int StartIndex, int EndIndex)> FoundObjectInitializersRanges = [];

    public NameResolver NameResolver;

    public SharedPassContext()
    {
        NameResolver = new NameResolver(this);
    }

    public void MarkNodeAs(Node node, string colour, bool skipIdentifierPostProcess = false)
    {
        if (node.IsChain)
            throw new Exception("Unexpected chain");

        Logger.Info($"Marking '{node.Text}' as '{colour}'");

        if (!node.SkipIdentifierPostProcessing)
        {
            node.Colour = colour;
            node.ClassificationType = NameResolver.MapColourToClassificationType(colour, node.ClassificationType);
            node.SkipIdentifierPostProcessing = skipIdentifierPostProcess;
        }

        UpdateStats(node.Colour, node.Text);
    }

    public void UpdateStats(string colour, string text)
    {
        if (colour == NodeColors.Class)
            FoundClasses.Add(text);

        if (colour == NodeColors.Struct)
            FoundStructs.Add(text);

        if (colour == NodeColors.Interface)
            FoundInterfaces.Add(text);

        if (colour == NodeColors.Namespace)
            FoundNamespaceParts.Add(text);
    }
}