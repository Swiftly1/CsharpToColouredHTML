﻿using System.Text;

namespace CsharpToColouredHTML.Core
{
    internal class CSSProvider
    {
        private static string DEFAULT_CSS_COLORS = string.Empty;
        private string? UserProvidedCSS;

        public CSSProvider(string? userProvidedCSS)
        {
            UserProvidedCSS = userProvidedCSS;

            // Generate CSS from ColorsMap at first run
            if (string.IsNullOrEmpty(DEFAULT_CSS_COLORS))
                DEFAULT_CSS_COLORS = GenerateDefaultCSSColors();
        }

        public string GetMappedColour(string s) => ColorsMap[s];

        public string GetCSS(bool addLineNumber, string mostCommonColourValue)
        {
            if (UserProvidedCSS != null)
            {
                return UserProvidedCSS;
            }

            var background = ApplyMostCommonColourToTemplateCSS(Background_CSS_Template, mostCommonColourValue);
            var _sb = new StringBuilder();
            _sb.AppendLine("<style>");
            _sb.Append(background);
            _sb.AppendLine(DEFAULT_CSS_COLORS);

            if (addLineNumber)
            {
                _sb.AppendLine(ApplyMostCommonColourToTemplateCSS(LineNumbers_CSS_Template, mostCommonColourValue));
            }

            _sb.AppendLine("</style>");
            return new string(_sb.ToString().Where(x => !char.IsWhiteSpace(x)).ToArray());
        }

        private string GenerateDefaultCSSColors()
        {
            var template =
            @".{0}
            {{
                color: {1};
            }}";

            var sb = new StringBuilder();
            foreach (var entry in ColorsMap)
                sb.Append(string.Format(template, entry.Key, entry.Value));

            sb.AppendLine();

            return sb.ToString();
        }

        private string ApplyMostCommonColourToTemplateCSS(string template, string colour_value)
        {
            return template.Replace("_PLACEHOLDER_", colour_value);
        }

        private Dictionary<string, string> ColorsMap = new Dictionary<string, string>
        {
            { InternalHtmlColors.Numeric, "#b5cea8" },
            { InternalHtmlColors.Method, "#DCDCAA" },
            { InternalHtmlColors.Class, "#4EC9B0" },
            { InternalHtmlColors.Keyword, "#569cd6" },
            { InternalHtmlColors.String, "#ce9178" },
            { InternalHtmlColors.Interface, "#b8d7a3" },
            { InternalHtmlColors.EnumName, "#b8d7a3" },
            { InternalHtmlColors.NumericLiteral, "#b8d7a3" },
            { InternalHtmlColors.RecordStructName, "#b8d7a3" },
            { InternalHtmlColors.TypeParameterName, "#b8d7a3" },
            { InternalHtmlColors.ExtensionMethodName, "#b8d7a3" },
            { InternalHtmlColors.Control, "#C586C0" },
            { InternalHtmlColors.InternalError, "#FF0D0D" },
            { InternalHtmlColors.Comment, "#6A9955" },
            { InternalHtmlColors.Preprocessor, "#808080" },
            { InternalHtmlColors.PreprocessorText, "#a4a4a4" },
            { InternalHtmlColors.Struct, "#86C691" },
            { InternalHtmlColors.Namespace, "#dfdfdf" },
            { InternalHtmlColors.EnumMemberName, "#dfdfdf" },
            { InternalHtmlColors.Identifier, "#dfdfdf" },
            { InternalHtmlColors.Punctuation, "#dfdfdf" },
            { InternalHtmlColors.Operator, "#dfdfdf" },
            { InternalHtmlColors.PropertyName, "#dfdfdf" },
            { InternalHtmlColors.FieldName, "#dfdfdf" },
            { InternalHtmlColors.LabelName, "#dfdfdf" },
            { InternalHtmlColors.OperatorOverloaded, "#dfdfdf" },
            { InternalHtmlColors.ConstantName, "#dfdfdf" },
            { InternalHtmlColors.LocalName, "#9CDCFE" },
            { InternalHtmlColors.ParameterName, "#9CDCFE" },
        };

        private const string Background_CSS_Template =
        $@".{InternalHtmlColors.Background}
        {{
            font-family: monaco,Consolas,Lucida Console,monospace; 
            background-color: #1E1E1E;
            overflow:scroll;
            color: _PLACEHOLDER_;
        }}";

        private const string LineNumbers_CSS_Template =
        @$"
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
            color: _PLACEHOLDER_;
        }}
        ";
    }
}
