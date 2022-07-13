using System.Web;
using System.Text;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

public class HTMLEmitter : IEmitter
{
    /// <summary>
    /// Assumed Settings:
    ///     Default CSS,
    ///     Adding Line Numbers,
    ///     Optimization of generated HTML
    /// </summary>
    public HTMLEmitter()
    {
        var settings = new HTMLEmitterSettings();
        _cssHelper = new CSSProvider(settings.UserProvidedCSS);
        AddLineNumber = settings.AddLineNumber;
        Optimize = settings.Optimize;
    }

    public HTMLEmitter(HTMLEmitterSettings settings)
    {
        _cssHelper = new CSSProvider(settings.UserProvidedCSS);
        AddLineNumber = settings.AddLineNumber;
        Optimize = settings.Optimize;
    }

    // Internal Stuff:
    // This method is at the top, so I don't have to update link to the line in README.
    private string Escape(string textWithTrivia)
    {
        var escaped = HttpUtility.HtmlEncode(textWithTrivia);
        return escaped;
    }

    private readonly CSSProvider _cssHelper;

    private readonly bool AddLineNumber = true;

    private readonly bool Optimize = true;

    private bool _IsUsing = false;

    private bool _IsNew = false;

    private int _ParenthesisCounter = 0;

    private int _LineCounter = 0;

    private string _MostCommonColourValue = string.Empty;

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

    public void Emit(List<Node> input)
    {
        Reset();
        var nodes = Preprocess(input);
        Text = AddLineNumber ? GenerateHtmlWithLineNumbers(nodes) : GenerateHtml(nodes);
    }

    // Implementation:

    private void Reset()
    {
        Text = "";
        _IsUsing = false;
        _IsNew = false;
        _ParenthesisCounter = 0;
        _LineCounter = 0;
    }

    private List<NodeWithDetails> Preprocess(List<Node> nodes)
    {
        var list = new List<NodeWithDetails>();

        for (int i = 0; i < nodes.Count; i++)
        {
            var colour = ExtractColourAndSetMetaData(i, nodes);
            var nodeWithDetails = new NodeWithDetails
            (
                colour: colour,
                text: nodes[i].Text,
                trivia: nodes[i].Trivia,
                hasNewLine: nodes[i].HasNewLine,
                isNew: _IsNew,
                isUsing: _IsUsing,
                parenthesisCounter: _ParenthesisCounter
            );
            list.Add(nodeWithDetails);
        }

        // Optimizer - Merges Nodes with the same colour
        if (Optimize)
        {
            var mostCommonColourName = list.Select(x => x.Colour).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            _MostCommonColourValue = _cssHelper.GetMappedColour(mostCommonColourName);

            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];

                var mappedColour = _cssHelper.GetMappedColour(current.Colour);
                if (mappedColour == _MostCommonColourValue)
                    list[i].UsesMostCommonColour = true;

                if (i + 1 >= list.Count)
                    break;

                var next = list[i + 1];
                var mergable = current.Colour == next.Colour && !next.Trivia.Contains(Environment.NewLine);

                if (!mergable)
                    continue;

                list[i] = MergeNodes(current, next);
                list.RemoveAt(i + 1);
                i--;
            }
        }

        return list;
    }

    private string GenerateHtml(List<NodeWithDetails> nodes)
    {
        var sb = new StringBuilder();

        sb.AppendLine(_cssHelper.GetCSS(AddLineNumber, Optimize, nodes, _MostCommonColourValue));
        sb.AppendLine(@"<pre class=""background"">");

        for (int i = 0; i < nodes.Count; i++)
        {
            var current = nodes[i];
            var postProcessed = PostProcessing(current);

            var span = current.UsesMostCommonColour ?
                $"{postProcessed.Before}{postProcessed.Content}{postProcessed.After}" :
                @$"{postProcessed.Before}<span class=""{current.Colour}"">{postProcessed.Content}</span>{postProcessed.After}";

            sb.Append(span);
        }

        sb.AppendLine("</pre>");

        return sb.ToString();
    }

    private string GenerateHtmlWithLineNumbers(List<NodeWithDetails> nodes)
    {
        var sb = new StringBuilder();

        sb.AppendLine(_cssHelper.GetCSS(AddLineNumber, Optimize, nodes, _MostCommonColourValue));
        sb.AppendLine(@"<pre class=""background"">");

        var isOpened = false;

        sb.AppendLine("<table>");
        sb.AppendLine("<tbody>");

        for (int i = 0; i < nodes.Count; i++)
        {
            var current = nodes[i];
            if (i == 0 || current.HasNewLine)
            {
                if (isOpened)
                {
                    sb.Append("</td></tr>");
                }

                AddRowsForNewLinesIfNeededToStringBuilder(current.Trivia, sb);

                sb.Append("<tr>");
                AddNewLineNumberToStringBuilder(sb);
                sb.Append("<td class=\"code_column\">");
                isOpened = true;
            }

            var postProcessed = PostProcessing(current);

            var span = current.UsesMostCommonColour ?
                $"{postProcessed.Before}{postProcessed.Content}{postProcessed.After}" :
                @$"{postProcessed.Before}<span class=""{current.Colour}"">{postProcessed.Content}</span>{postProcessed.After}";

            sb.Append(RemoveNewLines(span));
        }

        if (isOpened)
        {
            sb.Append("</td></tr>");
        }

        sb.AppendLine("</tbody>");
        sb.Append("</table>");
        sb.AppendLine("</pre>");

        return sb.ToString();
    }

    private string ExtractColourAndSetMetaData(int currentIndex, List<Node> nodes)
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
            colour = InternalHtmlColors.Namespace;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumName)
        {
            colour = InternalHtmlColors.EnumName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.EnumMemberName)
        {
            colour = InternalHtmlColors.EnumMemberName;
        }
        else if (BuiltInTypes.Contains(node.Text))
        {
            _IsNew = false;
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
                if (node.Text.Length > 0 && char.IsLower(node.Text[0]))
                {
                    colour = InternalHtmlColors.LocalName;
                }
                else
                {
                    colour = InternalHtmlColors.Class;
                }
            }
            else if (IsStruct(currentIndex, nodes))
            {
                colour = InternalHtmlColors.Struct;
            }
            else
            {
                colour = InternalHtmlColors.Identifier;
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
            colour = InternalHtmlColors.LocalName;
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

            colour = InternalHtmlColors.Punctuation;
        }
        else if (node.ClassificationType == ClassificationTypeNames.Operator)
        {
            colour = InternalHtmlColors.Operator;
        }
        else if (node.ClassificationType == ClassificationTypeNames.PropertyName)
        {
            colour = InternalHtmlColors.PropertyName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ParameterName)
        {
            colour = InternalHtmlColors.ParameterName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.FieldName)
        {
            colour = InternalHtmlColors.FieldName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.NumericLiteral)
        {
            colour = InternalHtmlColors.NumericLiteral;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
        {
            colour = InternalHtmlColors.Control;
        }
        else if (node.ClassificationType == ClassificationTypeNames.LabelName)
        {
            colour = InternalHtmlColors.LabelName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.OperatorOverloaded)
        {
            colour = InternalHtmlColors.OperatorOverloaded;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
        {
            colour = InternalHtmlColors.RecordStructName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
        {
            colour = InternalHtmlColors.Class;
        }
        else if (node.ClassificationType == ClassificationTypeNames.TypeParameterName)
        {
            colour = InternalHtmlColors.TypeParameterName;
        }
        else if (node.ClassificationType.Contains("xml doc comment"))
        {
            colour = InternalHtmlColors.Comment;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ExtensionMethodName)
        {
            colour = InternalHtmlColors.ExtensionMethodName;
        }
        else if (node.ClassificationType == ClassificationTypeNames.ConstantName)
        {
            colour = InternalHtmlColors.ConstantName;
        }

        return colour;
    }

    private string RemoveNewLines(string span)
    {
        return span.Replace(Environment.NewLine, "");
    }

    private void AddNewLineNumberToStringBuilder(StringBuilder sb)
    {
        var value = _LineCounter++;
        sb.Append($"<td class=\"line_no\" line_no=\"{value}\"></td>");
    }

    private void AddRowsForNewLinesIfNeededToStringBuilder(string trivia, StringBuilder sb)
    {
        var newLinesCount = StringHelper.AllIndicesOf(trivia, Environment.NewLine).Count;

        for (int i = newLinesCount - 1; i > 0; i--)
        {
            sb.Append("<tr>");
            AddNewLineNumberToStringBuilder(sb);
            sb.Append("<td>");
            sb.Append("</tr>");
        }
    }

    private (string Before, string Content, string After) PostProcessing(NodeWithDetails node)
    {
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

        var textWithReplacedTabs = processed_Text.Replace("\t", "    ");

        if (!textWithReplacedTabs.Any(char.IsWhiteSpace))
            return (string.Empty, Escape(textWithReplacedTabs), string.Empty);

        var before = string.Join(string.Empty, textWithReplacedTabs.TakeWhile(char.IsWhiteSpace));
        var after = string.Join(string.Empty, textWithReplacedTabs.Reverse().TakeWhile(char.IsWhiteSpace));

        if (before.Length == textWithReplacedTabs.Length)
            return (string.Empty, Escape(before), string.Empty);

        var length = textWithReplacedTabs.Length - before.Length - after.Length;
        var content = textWithReplacedTabs.Substring(before.Length, length);

        return (Escape(before), Escape(content), Escape(after));
    }

    private NodeWithDetails MergeNodes(NodeWithDetails current, NodeWithDetails next)
    {
        var newText = current.Text + next.TextWithTrivia;
        var newTrivia = current.Trivia;

        var details = new NodeWithDetails
        (
            colour: current.Colour,
            text: newText,
            trivia: newTrivia,
            hasNewLine: current.HasNewLine,
            isNew: current.IsNew,
            isUsing: current.IsUsing,
            parenthesisCounter: current.ParenthesisCounter
        );

        return details;
    }

    private bool IsStruct(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];
        var canGoBehind = currentIndex > 0;

        var isPopularStruct = IsPopularStruct(node.Text);

        if (isPopularStruct && !canGoBehind)
        {
            return true;
        }

        if (isPopularStruct && canGoBehind && nodes[currentIndex - 1].Text != ".")
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
        bool isPopularClass = false;

        if (canGoBehind && nodes[currentIndex - 1].Text == ":")
        {
            return true;
        }
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return true;
        }
        else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
        {
            return true;
        }
        else if (_IsNew && ThereIsMethodCallAhead(currentIndex, nodes))
        {
            _IsNew = false;
            return true;
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return true;
        } // be careful, if you remove those parenthesis around that assignment, then it'll change its behaviour
        else if ((isPopularClass = IsPopularClass(node.Text)) && !canGoBehind)
        {
            return true;
        }
        else if (isPopularClass && canGoBehind && nodes[currentIndex - 1].Text != ".")
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
            _IsNew = false;
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

        if (currentIndex + 3 >= nodes.Count)
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
        return new string[] { ")", "(", "=", ";", "}", ",", "&", "&&", "|", "||" }.Contains(next.Text);
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
}