namespace CsharpToColouredHTML.Extractor;

public static class MarkdownHelper
{
    public static List<string> ExtractCodeFromMarkdown(string s, string openingStr = "```csharp", string closingStr = "```")
    {
        var beginningIndices = AllIndexesOf(s, openingStr).Select(x => x + openingStr.Length).ToList();
        var closingIndices = AllIndexesOf(s, closingStr);
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

    private static List<int> AllIndexesOf(string str, string substr, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(substr))
        {
            throw new ArgumentException("String or substring is not specified.");
        }

        var indexes = new List<int>();
        int index = 0;

        while ((index = str.IndexOf(substr, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
        {
            indexes.Add(index++);
        }

        return indexes;
    }
}