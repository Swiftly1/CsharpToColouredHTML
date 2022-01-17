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
        NewLineCount = StringHelper.AllIndicesOf(TextWithTrivia, Environment.NewLine).Count;
    }

    public Node(string currentClassificationType, string text, string trivia, bool hasNewLine, int newLineCount)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
        HasNewLine = hasNewLine;
        NewLineCount = newLineCount;
    }

    public string TextWithTrivia { get; }

    public string ClassificationType { get; }

    public string Text { get; }

    public string Trivia { get; }

    public bool HasNewLine { get;  }

    public int NewLineCount { get; }

    public override string ToString()
    {
        return $"{ClassificationType} {Text}";
    }
}
