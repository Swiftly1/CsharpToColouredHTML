using System.Text;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

public class HTMLEmitter : IEmitter
{
    private readonly StringBuilder _sb = new StringBuilder();

    public List<string> BuiltInTypes { get; } = new List<string>
    {
        "bool",
        "byte",
        "sbyte",
        "char",
        "decimal",
        "double",
        "float",
        "int",
        "uint",
        "nint",
        "nuint",
        "long",
        "ulong",
        "short",
        "ushort",
        "object",
        "string",
        "dynamic",
    };

    public string Text { get; private set; }

    public void Emit(List<Node> nodes)
    {
        Text = "";
        _sb.Clear();

        AddCSS();
        _sb.AppendLine(@"<pre class=""background"">");

        for (int i = 0; i < nodes.Count; i++)
        {
            EmitNode(i, nodes);
        }

        _sb.AppendLine("<pre>");

        Text = _sb.ToString();
    }

    private bool _IsUsing = false;

    private bool _IsNew = false;

    private int _ParenthesisCounter = 0;

    public void EmitNode(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var colour = "";

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
        {
            colour = InternalHtmlColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            var canGoAhead = nodes.Count > currentIndex + 1;
            var canGoBehind = currentIndex > 0;

            if (node.Text.StartsWith("I"))
            {
                colour = InternalHtmlColors.Interface;
            }
            else if (BuiltInTypes.Contains(node.Text))
            {
                colour = InternalHtmlColors.Blue;
            }
            else if (canGoBehind && nodes[currentIndex - 1].Text == ":")
            {
                colour = InternalHtmlColors.Class;
            }
            else if (canGoAhead && nodes[currentIndex + 1].Text == "(")
            {
                if (_IsNew)
                {
                    colour = InternalHtmlColors.Class;
                }
                else
                {
                    colour = InternalHtmlColors.Method;
                }
            }
            else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
            {
                colour = InternalHtmlColors.Class;
            }
            else if (canGoBehind && nodes[currentIndex - 1].Text == ".")
            {
                if (_IsUsing)
                {
                    colour = InternalHtmlColors.White;
                }
                else
                {
                    colour = InternalHtmlColors.Method;
                }
            }
            else if (ThereIsMethodCallAhead(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Class;
            }
            else
            {
                colour = InternalHtmlColors.White;
            }
        }
        else if (node.ClassificationType == ClassificationTypeNames.Keyword)
        {
            if (node.Text == "using")
                _IsUsing = true;

            if (node.Text == "new")
                _IsNew = true;

            colour = InternalHtmlColors.Modifier;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            colour = InternalHtmlColors.String;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LocalName)
        {
            colour = InternalHtmlColors.VariableName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.MethodName)
        {
            colour = InternalHtmlColors.Method;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            if (node.Text == "(")
                _ParenthesisCounter++;

            if (node.Text == ")")
            {
                _ParenthesisCounter--;

                if (_ParenthesisCounter <= 0 && _IsUsing)
                    _IsUsing = false;

                if (_ParenthesisCounter <= 0 && _IsNew)
                    _IsNew = false;
            }

            if (node.Text == ";")
            {
                _IsUsing = false;
                _IsNew = false;
            }

            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Operator)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            colour = InternalHtmlColors.Control;
        }

        var span = @$"<span class=""{colour}"">{node.TextWithTrivia}</span>";
        _sb.Append(span);
    }

    private bool ThereIsMethodCallAhead(int currentIndex, List<Node> nodes)
    {
        // there's method call ahead so I guess that's an class, orrr namespace :(

        var i = currentIndex;
        var state = 0;

        while (++i < nodes.Count)
        {
            var current = nodes[i];

            if (state == 0 && current.ClassificationType == ClassificationTypeNames.Operator)
            {
                state = 1;
            }
            else if (state == 1 && current.ClassificationType == ClassificationTypeNames.Identifier)
            {
                state = 0;
            }
            else if (current.Text == "(")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    private void AddCSS()
    {
        _sb.AppendLine("<style>");
        _sb.AppendLine(new string(CSS.Where(c => !char.IsWhiteSpace(c)).ToArray()));
        _sb.AppendLine("</style>");
    }

    public const string CSS =
    @$".{InternalHtmlColors.Background}
    {{
        font-family: monaco,Consolas,Lucida Console,monospace; 
        background-color: #1E1E1E;
    }}

    .{InternalHtmlColors.Numeric}
    {{
        color: #b5cea8;
    }}

    .{InternalHtmlColors.Method}
    {{
        color: #DCDCAA;
    }}
  
    .{InternalHtmlColors.Class}
    {{
        color: #4EC9B0;
    }}
  
    .{InternalHtmlColors.Modifier}
    {{
        color: #569cd6;
    }}
  
    .{InternalHtmlColors.VariableName}
    {{
        color: #9CDCFE;
    }}  

    .{InternalHtmlColors.White}
    {{
        color: #D4D4D4;
    }}

    .{InternalHtmlColors.String}
    {{
        color: #ce9178;
    }}

    .{InternalHtmlColors.Interface}
    {{
        color: #b8d7a3;
    }}

    .{InternalHtmlColors.Control}
    {{
        color: #C586C0;
    }}
    ";

    private static class InternalHtmlColors
    {
        public const string Background = "background";

        public const string Numeric = "numeric";

        public const string Method = "method";

        public const string Class = "class";

        public const string Modifier = "modifier";

        public const string VariableName = "variableName";

        public const string White = "white";

        public const string String = "string";

        public const string Blue = "blue";

        public const string Control = "control";

        public const string Interface = "interface";
    }
}