namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class PassHelpers
{
    public static bool NameLikeInterface(string text)
    {
        return text.StartsWith("I") && text.Length > 1 && char.IsUpper(text[1]);
    }

    public static bool IsValidClassOrStructName(string text, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.Skip(1).All(x => char.IsLetter(x) || char.IsNumber(x) || x == '_');
    }

    public static readonly string[] CommonKeywordsBeforeTypeName =
    [
        "public", "private", "internal", "sealed", "protected", "readonly", "static", "override", "event", "required",
        "virtual", "unsafe", "partial", "delegate"
    ];

    public static readonly List<string> AccessibilityModifiers = new List<string>
    {
        "public", "private", "protected", "internal", "protected internal", "private protected"
    };

    public static readonly List<string> Operators = new List<string>
    {
        "+", "-", "/", "*", "=", "==", "+=", "-=", "*=", "/=", "!=", "&",
        "^", "|", "&&", "||", "??", "%=", "|=", "^=", "<<=", ">>=", "??=",
        ">>>", ">>>=", "<", ">", "is", "as", ">="
    };

    public bool SoundsLikeEventOrHandler(string name)
    {
        return name.StartsWith("On") || name.EndsWith("Event") || name.EndsWith("Handler") || name.EndsWith("Changed");
    }
}
