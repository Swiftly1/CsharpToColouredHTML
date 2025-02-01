using System.Text;
using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.Emitters.HTML
{
    internal class CSSProvider
    {
        private string? UserProvidedCSS;

        public CSSProvider(string? userProvidedCSS)
        {
            UserProvidedCSS = userProvidedCSS;
        }

        public string GetMappedColour(string s) => ColorsMap[s];

        public string GetCSS(bool addLineNumber, bool optimize, List<NodeAfterProcessing> nodes, string mostCommonColourValue)
        {
            if (UserProvidedCSS != null)
            {
                return UserProvidedCSS;
            }

            // If we're generating HTML with line numbers, then colour will be defined in other CSS Template
            var backgroundColour = addLineNumber ? string.Empty : mostCommonColourValue;
            var background = ApplyMostCommonColourToTemplateCSS(Background_CSS_Template, backgroundColour);
            var _sb = new StringBuilder();
            _sb.AppendLine("<style>");
            _sb.Append(background);
            _sb.AppendLine(GenerateDefaultCSSColors(optimize, nodes));

            if (addLineNumber)
            {
                _sb.AppendLine(ApplyMostCommonColourToTemplateCSS(LineNumbers_CSS_Template, mostCommonColourValue));
            }

            _sb.AppendLine("</style>");
            return new string(_sb.ToString().Where(x => !char.IsWhiteSpace(x)).ToArray());
        }

        private string GenerateDefaultCSSColors(bool optimize, List<NodeAfterProcessing> nodes)
        {
            var template =
            @".{0}
            {{
                color: {1};
            }}";

            var sb = new StringBuilder();

            var neededColors = ColorsMap.ToList();

            if (optimize)
                neededColors = neededColors.Where(colour => nodes.Any(n => n.Colour == colour.Key)).ToList();

            foreach (var entry in neededColors)
                sb.Append(string.Format(template, entry.Key, entry.Value));

            sb.AppendLine();

            return sb.ToString();
        }

        private string ApplyMostCommonColourToTemplateCSS(string template, string colour_value)
        {
            var cssValue = string.IsNullOrEmpty(colour_value) ? string.Empty : $"color: {colour_value};";
            return template.Replace("_PLACEHOLDER_", cssValue);
        }

        private Dictionary<string, string> ColorsMap = new Dictionary<string, string>
        {
            { NodeColors.Numeric, "#b5cea8" },
            { NodeColors.Method, "#DCDCAA" },
            { NodeColors.Class, "#4EC9B0" },
            { NodeColors.Keyword, "#569cd6" },
            { NodeColors.String, "#ce9178" },
            { NodeColors.Interface, "#b8d7a3" },
            { NodeColors.EnumName, "#b8d7a3" },
            { NodeColors.NumericLiteral, "#b8d7a3" },
            { NodeColors.RecordStructName, "#b8d7a3" },
            { NodeColors.TypeParameterName, "#b8d7a3" },
            { NodeColors.ExtensionMethodName, "#DCDCAA" },
            { NodeColors.Control, "#C586C0" },
            { NodeColors.InternalError, "#FF0D0D" },
            { NodeColors.Comment, "#6A9955" },
            { NodeColors.Preprocessor, "#808080" },
            { NodeColors.PreprocessorText, "#a4a4a4" },
            { NodeColors.Struct, "#86C691" },
            { NodeColors.Namespace, "#dfdfdf" },
            { NodeColors.EnumMemberName, "#dfdfdf" },
            { NodeColors.Identifier, "#dfdfdf" },
            { NodeColors.Default, "#dfdfdf" },
            { NodeColors.Punctuation, "#dfdfdf" },
            { NodeColors.Operator, "#dfdfdf" },
            { NodeColors.PropertyName, "#dfdfdf" },
            { NodeColors.FieldName, "#dfdfdf" },
            { NodeColors.LabelName, "#dfdfdf" },
            { NodeColors.OperatorOverloaded, "#dfdfdf" },
            { NodeColors.ConstantName, "#dfdfdf" },
            { NodeColors.LocalName, "#9CDCFE" },
            { NodeColors.ParameterName, "#9CDCFE" },
            { NodeColors.Delegate, "#4EC9B0" },
            { NodeColors.EventName, "#dfdfdf" },
            { NodeColors.ExcludedCode, "#808080" },
        };

        private const string Background_CSS_Template =
        $@".{NodeColors.Background}
        {{
            font-family: monaco,Consolas,Lucida Console,monospace; 
            background-color: #1E1E1E;
            overflow:scroll;
            _PLACEHOLDER_
        }}";

        private const string LineNumbers_CSS_Template =
        @$"
        .code_highlight
        {{
            background-color: #395929;
        }}

        table
        {{
            white-space: pre;
        }}

        .line_no::before
        {{
            content: attr(line_no);
            color: white;
        }}   

        .code_column
        {{
            padding-left: 5px;
            _PLACEHOLDER_
        }}
        ";
    }
}
