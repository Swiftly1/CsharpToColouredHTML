using System.Web;
using System.Text;

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
        UseIframe = settings.UseIframe;
    }

    public HTMLEmitter(HTMLEmitterSettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _cssHelper = new CSSProvider(settings.UserProvidedCSS);
        AddLineNumber = settings.AddLineNumber;
        Optimize = settings.Optimize;
        UseIframe = settings.UseIframe;
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

    private readonly bool UseIframe = true;

    private int _LineCounter = 0;

    private string _MostCommonColourValue = string.Empty;

    // Public Stuff:

    public string Emit(List<NodeWithDetails> nodes)
    {
        Reset();

        // Optimizer - Merges Nodes with the same colour
        if (Optimize)
        {
            OptimizeNodes(nodes);
        }

        var html = AddLineNumber ? GenerateHtmlWithLineNumbers(nodes) : GenerateHtml(nodes);
        return UseIframe ? AddIframe(html) : html;
    }

    // Implementation:

    private void Reset()
    {
        _LineCounter = 0;
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

    private string AddIframe(string html)
    {
        var iframe_start = "<iframe onload=\"this.height=this.contentWindow.document.body.scrollHeight + 40;\" " +
                            "frameborder=0 height=500 width=100% srcdoc=\"";
        var iframe_end = "\"></iframe>";
        var escaped = Escape(html);

        return iframe_start + escaped + iframe_end;
    }

    private void OptimizeNodes(List<NodeWithDetails> nodes)
    {
        if (!nodes.Any())
            return;

        var mostCommonColourName = nodes.Select(x => x.Colour).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
        _MostCommonColourValue = _cssHelper.GetMappedColour(mostCommonColourName);

        for (int i = 0; i < nodes.Count; i++)
        {
            var current = nodes[i];

            var mappedColour = _cssHelper.GetMappedColour(current.Colour);
            if (mappedColour == _MostCommonColourValue)
                nodes[i].UsesMostCommonColour = true;

            if (i + 1 >= nodes.Count)
                break;

            var next = nodes[i + 1];
            var mergable = current.Colour == next.Colour && !next.Trivia.Contains(Environment.NewLine);

            if (!mergable)
                continue;

            nodes[i] = MergeNodes(current, next);
            nodes.RemoveAt(i + 1);
            i--;
        }
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
            parenthesisCounter: current.ParenthesisCounter,
            classificationType: $"merged_nodes_invalid",
            skipIdentifierPostProcessing: false
        );

        return details;
    }
}