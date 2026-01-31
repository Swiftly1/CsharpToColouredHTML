namespace CsharpToColouredHTML.Core.Nodes;

internal record NodeInternalRepresentation
{
    public NodeInternalRepresentation(string currentClassificationType, string text, string trivia)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        HasNewLine = (trivia + text).Contains(Environment.NewLine);
    }

    public NodeInternalRepresentation(string currentClassificationType, string text, string trivia, bool hasNewLine)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        HasNewLine = hasNewLine;
    }

    public NodeInternalRepresentation(
        string colour,
        string text,
        string trivia,
        bool hasNewLine,
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

    public NodeInternalRepresentation(
        string colour,
        string text,
        string trivia,
        bool hasNewLine,
        string classificationType,
        bool skipIdentifierPostProcessing,
        Guid id) : this(colour, text, trivia, hasNewLine, classificationType, skipIdentifierPostProcessing)
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

    public override string ToString()
    {
        return $"'{Text}' is {ClassificationType}";
    }
}