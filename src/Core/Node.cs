namespace CsharpToColouredHTML.Core;

public class Node
{
    public Node(string currentClassificationType, string text, string trivia)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
        HasNewLine = TextWithTrivia.Contains(Environment.NewLine);
    }

    public Node(string currentClassificationType, string text, string trivia, bool hasNewLine)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
        HasNewLine = hasNewLine;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string TextWithTrivia { get; }

    public string ClassificationType { get; private set; }

    public string Text { get; }

    public string Trivia { get; }

    public bool HasNewLine { get;  }

    public bool ModifyClassificationType(string newClassificationType)
    {
        ClassificationType = newClassificationType;
        return true;
    }

    public override string ToString()
    {
        return $"{ClassificationType} {Text}";
    }
}
