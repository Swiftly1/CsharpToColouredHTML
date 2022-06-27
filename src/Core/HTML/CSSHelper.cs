using System.Text;

namespace CsharpToColouredHTML.Core
{
    internal class CSSHelper
    {
        private static string DEFAULT_CSS = string.Empty;
        private string? UserProvidedCSS;

        public CSSHelper(string? userProvidedCSS)
        {
            UserProvidedCSS = userProvidedCSS;

            // Generate CSS from ColorsMap at first run
            if (string.IsNullOrEmpty(DEFAULT_CSS))
                DEFAULT_CSS = GenerateDefaultCSS();
        }

        public string GetCSS(bool addLineNumber)
        {
            if (UserProvidedCSS != null)
            {
                return UserProvidedCSS;
            }

            var _sb = new StringBuilder();
            _sb.AppendLine("<style>");
            _sb.AppendLine(new string(DEFAULT_CSS.Where(c => !char.IsWhiteSpace(c)).ToArray()));

            if (addLineNumber)
            {
                _sb.AppendLine(new string(LineNumbersCSS.Where(c => !char.IsWhiteSpace(c)).ToArray()));
            }

            _sb.AppendLine("</style>");
            return _sb.ToString();
        }

        private string GenerateDefaultCSS()
        {
            var template =
            @".{0}
            {{
                color: {1};
            }}";

            var sb = new StringBuilder();
            sb.Append(BACKGROUND_CSS);

            foreach (var entry in ColorsMap)
                sb.Append(string.Format(template, entry.Key, entry.Value));

            sb.AppendLine();

            return sb.ToString();
        }

        private Dictionary<string, string> ColorsMap = new Dictionary<string, string>
        {
            { InternalHtmlColors.Numeric, "#b5cea8" },
            { InternalHtmlColors.Method, "#DCDCAA" },
            { InternalHtmlColors.Class, "#4EC9B0" },
            { InternalHtmlColors.Keyword, "#569cd6" },
            { InternalHtmlColors.Blue, "#9CDCFE" },
            { InternalHtmlColors.String, "#ce9178" },
            { InternalHtmlColors.Interface, "#b8d7a3" },
            { InternalHtmlColors.Control, "#C586C0" },
            { InternalHtmlColors.InternalError, "#FF0D0D" },
            { InternalHtmlColors.Comment, "#6A9955" },
            { InternalHtmlColors.Preprocessor, "#808080" },
            { InternalHtmlColors.PreprocessorText, "#a4a4a4" },
            { InternalHtmlColors.Struct, "#86C691" },
        };

        public const string BACKGROUND_CSS =
        $@".{InternalHtmlColors.Background}
        {{
            font-family: monaco,Consolas,Lucida Console,monospace; 
            background-color: #1E1E1E;
            overflow:scroll;
            color: #dfdfdf;
        }}";

        public const string LineNumbersCSS =
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
            color: #dfdfdf;
        }}
        ";
    }
}
