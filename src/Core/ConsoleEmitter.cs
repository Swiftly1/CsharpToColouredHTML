using System.Text;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

public class ConsoleEmitter : IEmitter
{
    public ConsoleEmitter(bool addDiagnosticInfo = false)
    {
        this.addDiagnosticInfo = addDiagnosticInfo;
    }

    private readonly StringBuilder _sb = new StringBuilder();

    private readonly bool addDiagnosticInfo;

    public string Text { get; private set; }

    public void Emit(List<Node> nodes)
    {
        Console.ResetColor();
        Text = "";
        _sb.Clear();

        foreach (var node in nodes)
        {
            EmitNode(node);
        }

        Text = _sb.ToString();
    }

    public void EmitNode(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.ClassName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LocalName)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
        }
        else if (node.ClassificationType == ClassificationTypeNames.MethodName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Operator)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }
        else if (node.ClassificationType == ClassificationTypeNames.VerbatimStringLiteral)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }      
        else if (node.ClassificationType == ClassificationTypeNames.ParameterName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else
        {
            Console.ResetColor();
        }

        _sb.Append(node.TextWithTrivia);
        Console.Write($"{node.TextWithTrivia}");

        if (addDiagnosticInfo)
        {
            _sb.Append($"({node.ClassificationType})");
            Console.Write($"({node.ClassificationType})");
        }

        Console.ResetColor();
    }
}
