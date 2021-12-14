namespace CsharpToColouredHTML.Miscs;

/// <summary>
/// The purpose of this class is to replace C#'s code within markdown syntax
/// with C# code represented with coloured HTML.
/// e.g ```csharp var a = 5;``` => var a = 5; => html code.
/// WARNING: It assumes that the input is trusted! so be careful.
/// Here's more about the problems if you'll process input from untrusted sources e.g users.
/// https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
/// </summary>
public static class MarkdownHelper
{
    /// <summary>
    /// The purpose of this class is to replace C#'s code within markdown syntax
    /// with C# code represented with coloured HTML.
    /// e.g ```csharp var a = 5;``` => var a = 5; => html code.
    /// WARNING: It assumes that the input is trusted! so be careful.
    /// Here's more about the problems if you'll process input from untrusted sources e.g users.
    /// https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
    /// </summary>
    public static List<string> ReplaceCsharpMarkdownWithHTMLCode_Unsafe(string s, string openingStr = "```csharp", string closingStr = "```")
    {
        var beginningIndices = AllIndicesOf(s, openingStr).Select(x => x + openingStr.Length).ToList();
        var closingIndices = AllIndicesOf(s, closingStr);
        var pairs = MakePairs(beginningIndices, closingIndices);

        var result = new List<string>();

        foreach (var pair in pairs)
        {
            if (pair.Start < 0 || pair.End >= s.Length)
                continue;

            var length = pair.End - pair.Start;

            result.Add(s.Substring(pair.Start, length));
        }

        return result;
    }

    private static List<(int Start, int End)> MakePairs(List<int> beginningIndices, List<int> closingIndices)
    {
        var closingCopy = closingIndices.ToList();

        var pairs = new List<(int Start, int End)>();

        foreach (var openingIndex in beginningIndices)
        {
            for (int i = 0; i < closingCopy.Count; i++)
            {
                var current = closingCopy[i];
                if (openingIndex < current)
                {
                    pairs.Add((openingIndex, current));
                    // since indices are sorted, we're removing those from the beginning that
                    // should never be used because openingIndex will always be higher than them
                    closingCopy.RemoveRange(0, i + 1);
                    break;
                }
            }
        }

        return pairs;
    }

    private static List<int> AllIndicesOf(string str, string substr)
    {
        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(substr))
        {
            throw new ArgumentException("String or substring is not specified.");
        }

        var indexes = new List<int>();
        int index = 0;

        while ((index = str.IndexOf(substr, index)) != -1)
        {
            indexes.Add(index++);
        }

        return indexes;
    }
}