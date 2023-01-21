namespace CsharpToColouredHTML.Core;

public record NodeAfterProcessing
{
    public NodeAfterProcessing(
        Guid id,
        string colour,
        string text,
        string trivia,
        bool hasNewLine,
        string originalClassificationType,
        bool usesMostCommonColour
        )
    {
        Id = id;
        Colour = colour;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = Trivia + Text;
        HasNewLine = hasNewLine;
        OriginalClassificationType = originalClassificationType;
        UsesMostCommonColour = usesMostCommonColour;
    }

    public Guid Id { get; init; }

    public string Colour { get; set; }

    public string Text { get; init; }

    public string Trivia { get; init; }

    public string TextWithTrivia { get; init; }

    public bool HasNewLine { get; init; }

    public string OriginalClassificationType { get; init; }

    public bool UsesMostCommonColour { get; set; }
}