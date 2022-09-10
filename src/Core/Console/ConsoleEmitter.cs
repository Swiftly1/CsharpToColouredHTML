using System.Text;

namespace CsharpToColouredHTML.Core;

public class ConsoleEmitter : IEmitter
{
    public ConsoleEmitter(bool addDiagnosticInfo = false)
    {
        this.addDiagnosticInfo = addDiagnosticInfo;
    }

    private readonly StringBuilder _sb = new StringBuilder();

    private readonly bool addDiagnosticInfo;

    public string Emit(List<NodeWithDetails> nodes)
    {
        Console.ResetColor();
        _sb.Clear();

        foreach (var node in nodes)
        {
            EmitNode(node);
        }

        return _sb.ToString();
    }

    public void EmitNode(NodeWithDetails node)
    {
        if (node.Colour == NodeColors.Class)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (node.Colour == NodeColors.ParameterName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (node.Colour == NodeColors.Identifier)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        else if (node.Colour == NodeColors.TypeParameterName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (node.Colour == NodeColors.LocalName)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
        }
        else if (node.Colour == NodeColors.Punctuation)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
        }
        else if (node.Colour == NodeColors.Operator)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (node.Colour == NodeColors.String)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
        else if (node.Colour == NodeColors.Keyword)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (node.Colour == NodeColors.Comment)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
        }
        else if (node.Colour == NodeColors.Control)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }
        else if (node.Colour == NodeColors.Method)
        {
            Console.ForegroundColor = ConsoleColor.Green;
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
