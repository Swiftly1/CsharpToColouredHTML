namespace CsharpToColouredHTML.Core;

public record NodeAfterProcessing
{
    public NodeAfterProcessing(
        Guid id,
        string colour,
        string text,
        string trivia,
        string originalClassificationType,
        bool usesMostCommonColour,
        int lineNumber,
        bool useHighlighting
        )
    {
        Id = id;
        Colour = colour;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = Trivia + Text;
        OriginalClassificationType = originalClassificationType;
        UsesMostCommonColour = usesMostCommonColour;
        LineNumber = lineNumber;
        UseHighlighting = useHighlighting;
    }

    public Guid Id { get; init; }

    public string Colour { get; set; }

    public string Text { get; init; }

    public string Trivia { get; init; }

    public string TextWithTrivia { get; init; }

    public bool HasNewLine { get; init; }

    public string OriginalClassificationType { get; init; }

    public int LineNumber { get; init; }

    public bool UsesMostCommonColour { get; set; }

    public bool UseHighlighting { get; set; }
}