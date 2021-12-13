namespace CsharpToColouredHTML.Core;

public class Node
{
    public Node(string currentClassificationType, string text, string trivia)
    {
        ClassificationType = currentClassificationType;
        Text = text;
        Trivia = trivia;
        TextWithTrivia = trivia + text;
    }

    public string TextWithTrivia { get; }

    public string ClassificationType { get; }

    public string Text { get; }

    public string Trivia { get; }
}
