namespace CsharpToColouredHTML.Core.Nodes;

public record NodeWithDetails
{
    public NodeWithDetails(
        string colour,
        string text,
        string trivia,
        bool hasNewLine,
        int parenthesisCounter,
        string classificationType,
        bool skipIdentifierPostProcessing
        )
    {
        Colour = colour;
        Text = text;
        Trivia = trivia;
        HasNewLine = hasNewLine;
        ParenthesisCounter = parenthesisCounter;
        TextWithTrivia = Trivia + Text;
        ClassificationType = classificationType;
        Id = Guid.NewGuid();
        SkipIdentifierPostProcessing = skipIdentifierPostProcessing;
    }

    public NodeWithDetails(
        string colour,
        string text,
        string trivia,
        bool hasNewLine,
        int parenthesisCounter,
        string classificationType,
        bool skipIdentifierPostProcessing,
        Guid id) : this(colour, text, trivia, hasNewLine, parenthesisCounter, classificationType, skipIdentifierPostProcessing)
    {
        Id = id;
    }

    public Guid Id { get; init; }

    public string Colour { get; set; }

    public string Text { get; init; }

    public string Trivia { get; init; }

    public string TextWithTrivia { get; init; }

    public bool HasNewLine { get; init; }

    public bool IsNew { get; init; }

    public bool IsUsing { get; init; }

    public int ParenthesisCounter { get; init; }

    public string ClassificationType { get; set; }

    public bool UsesMostCommonColour { get; set; }

    public bool SkipIdentifierPostProcessing { get; set; }

    public int LineNumber { get; set; }
}