using Xunit;
using System.IO;
using System.Text;
using System.Collections.Generic;
using CsharpToColouredHTML.Miscs;

namespace Tests
{
    public class MarkDownHelperTests
    {
        private const string InputDir = $"InputMarkdown";
        private const string OutputDir = $"OutputMarkdown";
        private const string Splitter = $"_____________________________";

        [Theory]
        [InlineData("0001.txt")]
        [InlineData("0002.txt")]
        [InlineData("0003.txt")]
        public void Test1(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var p2 = Path.Combine(OutputDir, fileName);

            var markdown = File.ReadAllText(p1);
            var codes = SplitLinesBy(File.ReadAllLines(p2), Splitter);

            var extracted_codes = MarkdownHelper.ReplaceCsharpMarkdownWithHTMLCode_Unsafe(markdown);

            for (int i = 0; i < codes.Count; i++)
            {
                Assert.Equal(codes[i], extracted_codes[i]);
            }
        }

        private List<string> SplitLinesBy(string[] lines, string splitter)
        {
            var list = new List<string>();

            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (line == splitter)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
                list.Add(sb.ToString());

            return list;
        }
    }
}