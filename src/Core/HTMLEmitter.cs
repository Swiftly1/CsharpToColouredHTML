using System.Web;
using System.Text;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

public class HTMLEmitter : IEmitter
{
    public HTMLEmitter(string user_provided_css = null, bool addLineNumber = false)
    {
        UserProvidedCSS = user_provided_css;
        AddLineNumber = addLineNumber;
    }

    // Internal Stuff:

    private string Escape(string textWithTrivia)
    {
        var escaped = HttpUtility.HtmlEncode(textWithTrivia);
        return escaped;
    }

    private readonly StringBuilder _sb = new StringBuilder();

    private readonly string UserProvidedCSS = null;

    private readonly bool AddLineNumber = true;

    private bool _IsUsing = false;

    private bool _IsNew = false;

    private int _ParenthesisCounter = 0;

    private int _LineCounter = 0;

    // Public Stuff:

    public string Text { get; private set; }

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

    public List<string> ReallyPopularClasses { get; } = new List<string>
    {
        "List",
        "Dictionary",
        "Console",
        "Task",
        "Func",
        "Action",
        "Predicate",
        "EventArgs",
        "File"
    };

    public List<string> ReallyPopularClassSubstrings { get; } = new List<string>
    {
        "Controller",
        "DTO",
        "User",
        "Manager",
        "Handler",
        "Node",
        "Exception",
        "EventHandler"
    };

    public List<string> ReallyPopularStructs { get; } = new List<string>
    {
        "CancellationToken",
        "IEnumerable"
    };

    public List<string> ReallyPopularStructsSubstrings { get; } = new List<string>
    {
        "Span",
    };

    public void Emit(List<Node> nodes)
    {
        Reset();
        AddCSS();
        _sb.AppendLine(@"<pre class=""background"">");

        var isOpened = false;

        if (AddLineNumber)
        {
            _sb.AppendLine("<table>");
            _sb.AppendLine("<tbody>");
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (AddLineNumber)
            {
                var current = nodes[i];
                if (i == 0 || current.HasNewLine)
                {
                    if (isOpened)
                    {
                        _sb.Append("</td></tr>");
                    }

                    CreateRowsForNewLinesIfNeeded(current);

                    _sb.Append("<tr>");
                    AddNewLineNumber();
                    _sb.Append("<td class=\"code_column\">");
                    isOpened = true;
                }
            }

            var span = EmitNode(i, nodes);

            if (AddLineNumber)
            {
                _sb.Append(RemoveNewLines(span));
            }
            else
            {
                _sb.Append(span);
            }
        }

        if (AddLineNumber && isOpened)
        {
            _sb.Append("</td></tr>");
        }

        if (AddLineNumber)
        {
            _sb.AppendLine("</tbody>");
            _sb.Append("</table>");
        }

        _sb.AppendLine("</pre>");

        Text = _sb.ToString();
    }

    private string RemoveNewLines(string span)
    {
        return span.Replace(Environment.NewLine, "");
    }

    private void AddNewLineNumber()
    {
        var value = _LineCounter++;
        _sb.Append($"<td class=\"line_no\" line_no=\"{value}\"></td>");
    }

    private void CreateRowsForNewLinesIfNeeded(Node current)
    {
        var newLinesCount = StringHelper.AllIndicesOf(current.Trivia, Environment.NewLine).Count;

        for (int i = newLinesCount - 1; i > 0; i--)
        {
            _sb.Append("<tr>");
            AddNewLineNumber();
            _sb.Append("<td>");
            _sb.Append("</tr>");
        }
    }

    private void Reset()
    {
        Text = "";
        _sb.Clear();
        _IsUsing = false;
        _IsNew = false;
        _ParenthesisCounter = 0;
        _LineCounter = 0;
    }

    public string EmitNode(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var colour = InternalHtmlColors.InternalError;

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
        {
            colour = InternalHtmlColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Comment)
        {
            colour = InternalHtmlColors.Comment;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PreprocessorKeyword)
        {
            colour = InternalHtmlColors.Preprocessor;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PreprocessorText)
        {
            colour = InternalHtmlColors.PreprocessorText;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StructName)
        {
            colour = InternalHtmlColors.Struct;
        }
        else if (node.ClassificationType == ClassificationTypeNames.InterfaceName)
        {
            colour = InternalHtmlColors.Interface;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumName)
        {
            colour = InternalHtmlColors.Interface;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumMemberName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (BuiltInTypes.Contains(node.Text))
        {
            colour = InternalHtmlColors.Keyword;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Identifier)
        {
            if (IsInterface(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Interface;
            }
            else if (IsMethod(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Method;
            }
            else if (IsClass(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Class;
            }
            else if (IsStruct(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Struct;
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

            colour = InternalHtmlColors.Keyword;
        }
        else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
        {
            colour = InternalHtmlColors.String;
        }
        else if (node.ClassificationType == ClassificationTypeNames.VerbatimStringLiteral)
        {
            colour = InternalHtmlColors.String;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LocalName)
        {
            colour = InternalHtmlColors.Blue;
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
        else if (node.ClassificationType == ClassificationTypeNames.PropertyName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ParameterName)
        {
            colour = InternalHtmlColors.Blue;
        }
        else if (node.ClassificationType == ClassificationTypeNames.FieldName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NumericLiteral)
        {
            colour = InternalHtmlColors.Interface;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            colour = InternalHtmlColors.Control;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LabelName)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.OperatorOverloaded)
        {
            colour = InternalHtmlColors.White;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
        {
            colour = InternalHtmlColors.Interface;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
        {
            colour = InternalHtmlColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.TypeParameterName)
        {
            colour = InternalHtmlColors.Interface;
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            colour = InternalHtmlColors.Comment;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ExtensionMethodName)
        {
            colour = InternalHtmlColors.Method;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ConstantName)
        {
            colour = InternalHtmlColors.White;
        }

        var processed_Text = "";

        if (AddLineNumber)
        {
            var last = node.Trivia.Split(Environment.NewLine).Last();
            processed_Text = last + node.Text;
        }
        else
        {
            processed_Text = node.TextWithTrivia;
        }

        var escaped = Escape(processed_Text);
        var changed_tabs = escaped.Replace("\t", "    ");
        var span = @$"<span class=""{colour}"">{changed_tabs}</span>";
        return span;
    }

    private bool IsStruct(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        if (IsPopularStruct(node.Text))
        {
            return true;
        }

        return false;
    }

    private bool IsInterface(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        var startsWithI = node.Text.StartsWith("I");

        if (startsWithI && canGoBehind && new[] { ":", "<" }.Contains(nodes[currentIndex - 1].Text))
        {
            return true;
        }
        else if (startsWithI && canGoBehind && new[] { "public", "private", "internal", "sealed", "protected", "readonly" }.Contains(nodes[currentIndex - 1].Text))
        {
            return true;
        }
        else if (startsWithI && canGoAhead && nodes[currentIndex + 1].ClassificationType == ClassificationTypeNames.ParameterName)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsMethod(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        // [InlineData("0001.txt")]
        // var a = list[test()];
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "[")
        {
            var identifier = nodes[currentIndex - 2].ClassificationType;

            var hasIdentifierBefore = identifier == ClassificationTypeNames.Identifier ||
                identifier == ClassificationTypeNames.LocalName;

            if (!hasIdentifierBefore)
                return false;
        }

        if (!_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return true;
        }
        else if (_IsUsing && !_IsUsing && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsClass(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.Count > currentIndex + 1;
        var canGoBehind = currentIndex > 0;

        var node = nodes[currentIndex];

        if (canGoBehind && nodes[currentIndex - 1].Text == ":")
        {
            return true;
        }
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return true;
        }
        else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
        {
            return true;
        }
        else if (_IsNew && ThereIsMethodCallAhead(currentIndex, nodes))
        {
            return true;
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return true;
        }
        else if (IsPopularClass(node.Text))
        {
            return true;
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "<")
        {
            return true;
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "[")
        {
            return true;
        }
        // new DictionaryList<int, int>();
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            return true;
        }
        else if (nodes.Count > currentIndex + 4 &&
            nodes[currentIndex + 2].Text == "=" &&
            nodes[currentIndex + 3].Text == "new" &&
            nodes[currentIndex + 4].Text == node.Text)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool SeemsLikePropertyUsage(int currentIndex, List<Node> nodes)
    {
        if (_IsUsing)
            return false;

        if (currentIndex + 3 >= nodes.Count - 1)
            return false;

        var next = nodes[currentIndex + 1];

        if (next.ClassificationType != ClassificationTypeNames.Operator)
            return false;

        next = nodes[currentIndex + 2];

        if (next.ClassificationType != ClassificationTypeNames.Identifier)
            return false;

        next = nodes[currentIndex + 3];

        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.LocalName)
            return false;
        
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "." && nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.ParameterName)
            return false;

        // OLEMSGICON.OLEMSGICON_WARNING,
        return new string[] { ")", "(", "=", ";", "}", ",", "&", "&&", "|", "||"}.Contains(next.Text);
    }

    private bool IsPopularClass(string text)
    {
        return ReallyPopularClasses.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase))
            ||
            ReallyPopularClassSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPopularStruct(string text)
    {
        return ReallyPopularStructs.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase)) 
            ||
            ReallyPopularStructsSubstrings.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
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
        if (UserProvidedCSS != null)
        {
            _sb.AppendLine(UserProvidedCSS);
        }
        else
        {
            _sb.AppendLine("<style>");
            _sb.AppendLine(new string(DEFAULT_CSS.Where(c => !char.IsWhiteSpace(c)).ToArray()));

            if (AddLineNumber)
            {
                _sb.AppendLine(new string(LineNumbersCSS.Where(c => !char.IsWhiteSpace(c)).ToArray()));
            }

            _sb.AppendLine("</style>");
        }
    }

    public const string DEFAULT_CSS =
    @$".{InternalHtmlColors.Background}
    {{
        font-family: monaco,Consolas,Lucida Console,monospace; 
        background-color: #1E1E1E;
        overflow:scroll;
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
  
    .{InternalHtmlColors.Keyword}
    {{
        color: #569cd6;
    }}
  
    .{InternalHtmlColors.Blue}
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

    .{InternalHtmlColors.InternalError}
    {{
        color: #FF0D0D;
    }}

    .{InternalHtmlColors.Comment}
    {{
        color: #6A9955;
    }} 

    .{InternalHtmlColors.Preprocessor}
    {{
        color: #808080;
    }}

    .{InternalHtmlColors.PreprocessorText}
    {{
        color: #a4a4a4;
    }}

    .{InternalHtmlColors.Struct}
    {{
        color: #86C691;
    }}
    ";

    public const string LineNumbersCSS =
    @$"
    table
    {{
        color: white;
        white-space: pre;
    }}

    .line_no::before
    {{
        content: attr(line_no);
    }}   

    .code_column
    {{
        padding-left: 5px;
    }}
    ";
}