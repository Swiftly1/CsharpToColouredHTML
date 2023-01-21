namespace CsharpToColouredHTML.Core;

internal struct ExtractedColourResult
{
    public ExtractedColourResult(string colour)
    {
        Colour = colour;
        SkipIdentifierPostProcessing = false;
    }

    public ExtractedColourResult(string colour, bool skipIdentifierPostProcessing)
    {
        Colour = colour;
        SkipIdentifierPostProcessing = skipIdentifierPostProcessing;
    }

    public string Colour { get; set; }

    public bool SkipIdentifierPostProcessing { get; set; }
}