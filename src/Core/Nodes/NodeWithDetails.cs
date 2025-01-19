namespace CsharpToColouredHTML.Core.Nodes;

internal record NodeWithDetails
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

    public bool HasNewLine { get; init; }

    public string ClassificationType { get; set; }

    public bool UsesMostCommonColour { get; set; }

    public bool SkipIdentifierPostProcessing { get; set; }

    public int LineNumber { get; set; }
}