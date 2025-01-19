using System.Web;
using System.Text;
using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.Emitters.HTML;

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
        HighlightingPredicate = settings.LineHighlightingPredicate;
    }

    public HTMLEmitter(HTMLEmitterSettings settings)
    {
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));

        _cssHelper = new CSSProvider(settings.UserProvidedCSS);
        AddLineNumber = settings.AddLineNumber;
        Optimize = settings.Optimize;
        UseIframe = settings.UseIframe;
        HighlightingPredicate = settings.LineHighlightingPredicate;
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

    private readonly Func<int, bool>? HighlightingPredicate = null;

    private int _LineCounter = 1;

    private string _MostCommonColourValue = string.Empty;

    // Public Stuff:

    public string Emit(List<NodeAfterProcessing> nodes)
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
        _LineCounter = 1;
    }

    private string GenerateHtml(List<NodeAfterProcessing> nodes)
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

    private string GenerateHtmlWithLineNumbers(List<NodeAfterProcessing> nodes)
    {
        var sb = new StringBuilder();

        sb.AppendLine(_cssHelper.GetCSS(AddLineNumber, Optimize, nodes, _MostCommonColourValue));
        sb.AppendLine(@"<pre class=""background"">");

        sb.AppendLine("<table>");
        sb.AppendLine("<tbody>");

        var isOpened = false;
        var currentLine = 0;
        var highlightCssName = "code_highlight";

        for (int i = 0; i < nodes.Count; i++)
        {
            var current = nodes[i];

            if (i == 0 || current.LineNumber != currentLine)
            {
                var gap = current.LineNumber - currentLine;
                currentLine = current.LineNumber;

                var highlighWholeRow =
                    (HighlightingPredicate != null && HighlightingPredicate(currentLine)) ||
                    nodes.Where(x => x.LineNumber == currentLine).All(x => x.UseHighlighting);

                if (isOpened)
                {
                    sb.Append("</td></tr>");
                }

                while (gap-- > 1)
                {
                    sb.Append("<tr>");

                    var higlightCss = string.Empty;

                    if (HighlightingPredicate != null && HighlightingPredicate(_LineCounter))
                    {
                        higlightCss = " class=\"code_highlight\"";
                    }

                    AddNewLineNumberToStringBuilder(sb);

                    sb.Append($"<td{higlightCss}>");
                    sb.Append("</tr>");
                }

                sb.Append("<tr>");
                AddNewLineNumberToStringBuilder(sb);

                var rowClassName = $"code_column{(highlighWholeRow ? $" {highlightCssName}" : string.Empty)}";
                sb.Append($"<td class=\"{rowClassName}\">");
                isOpened = true;
            }

            var postProcessed = PostProcessing(current);

            var className = $"{current.Colour}{(current.UseHighlighting ? $" {highlightCssName}" : string.Empty)}";

            var span = current.UsesMostCommonColour ?
                $"{postProcessed.Before}{postProcessed.Content}{postProcessed.After}" :
                @$"{postProcessed.Before}<span class=""{className}"">{postProcessed.Content}</span>{postProcessed.After}";

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

    private (string Before, string Content, string After) PostProcessing(NodeAfterProcessing node)
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

    private void OptimizeNodes(List<NodeAfterProcessing> nodes)
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

    private NodeAfterProcessing MergeNodes(NodeAfterProcessing current, NodeAfterProcessing next)
    {
        var newText = current.Text + next.TextWithTrivia;
        var newTrivia = current.Trivia;

        var details = new NodeAfterProcessing
        (
            Guid.NewGuid(),
            colour: current.Colour,
            text: newText,
            trivia: newTrivia,
            originalClassificationType: $"merged_nodes_invalid",
            usesMostCommonColour: current.UsesMostCommonColour,
            lineNumber: current.LineNumber,
            useHighlighting: current.UseHighlighting || next.UseHighlighting
        );

        return details;
    }
}