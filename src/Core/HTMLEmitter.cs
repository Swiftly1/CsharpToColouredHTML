using System.Text;
using Microsoft.CodeAnalysis.Classification;

namespace Core;

public class HTMLEmitter : IEmitter
{
    private readonly StringBuilder _sb = new StringBuilder();

    public string Text { get; private set; }

    public void Emit(List<Node> nodes)
    {
        Text = "";
        _sb.Clear();

        AddCSS();
        _sb.AppendLine(@"<pre class=""background"">");
        foreach (var node in nodes)
        {
            EmitNode(node);
        }
        _sb.AppendLine("<pre>");

        Text = _sb.ToString();
    }

    public void EmitNode(Node node)
    {
        var colour = "";
        if (node.ClassificationType == ClassificationTypeNames.ClassName)
        {
            colour = "class";
        }
        else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
        {
            colour = "white";
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            colour = "variableName";
        }
        else if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            colour = "modifier";
        }
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            colour = "string";
        }
        else if (node.ClassificationType == ClassificationTypeNames.LocalName)
        {
            colour = "variableName";
        }
        else if (node.ClassificationType == ClassificationTypeNames.MethodName)
        {
            colour = "method";
        }
        else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            colour = "white";
        }
        else if (node.ClassificationType == ClassificationTypeNames.Operator)
        {
            colour = "white";
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            colour = "control";
        }

        var span = @$"<span class=""{colour}"">{node.TextWithTrivia}</span>";
        _sb.Append(span);
    }

    private void AddCSS()
    {
        var css = 
      @".background
        {
            background-color: #1E1E1E;
        }

        .numeric
        {
            color: #b5cea8;
        }

        .method
        {
            color: #DCDCAA;
        }
  
        .class
        {
            color: #4EC9B0;
        }
  
        .modifier
        {
            color: #569cd6;
        }
  
        .variableName
        {
            color: #9CDCFE;
        }  

        .white
        {
            color: #D4D4D4;
        }

        .string
        {
            color: #ce9178;
        }

        .control
        {
            color: #C586C0;
        }";

        _sb.AppendLine("<style>");
        _sb.AppendLine(new string(css.Where(c => !char.IsWhiteSpace(c)).ToArray()));
        _sb.AppendLine("</style>");
    }
}