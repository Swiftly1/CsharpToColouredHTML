namespace CsharpToColouredHTML.Core.Nodes;

public class Node
{
    public Node(string currentClassificationType, string text, string trivia)
    {
        ClassificationType = currentClassificationType;
        OriginalClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
        HasNewLine = TextWithTrivia.Contains(Environment.NewLine);
    }

    public Node(string currentClassificationType, string text, string trivia, bool hasNewLine)
    {
        ClassificationType = currentClassificationType;
        OriginalClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
        HasNewLine = hasNewLine;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string TextWithTrivia { get; }

    public string ClassificationType { get; private set; }

    public string OriginalClassificationType { get; }

    public string Text { get; }

    public string Trivia { get; }

    public bool HasNewLine { get; }

    public bool HasAlreadyBeenMarked { get; set; } = false;

    public bool ModifyClassificationType(string newClassificationType)
    {
        HasAlreadyBeenMarked = true;
        ClassificationType = newClassificationType;
        return true;
    }

    public override string ToString()
    {
        return $"{ClassificationType} {Text}";
    }
}
