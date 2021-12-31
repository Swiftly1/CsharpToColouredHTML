using Xunit;
using System.IO;
using CsharpToColouredHTML.Core;

namespace Tests
{
    public class E2E
    {
        private const string InputDir = $"InputCsharp";
        private const string OutputDir = $"OutputHtml";

        [Theory]
        [InlineData("0001.txt")]
        [InlineData("0002.txt")]
        [InlineData("0003.txt")]
        [InlineData("0004.txt")]
        [InlineData("0005.txt")]
        [InlineData("0006.txt")]
        [InlineData("0007.txt")]
        [InlineData("0008.txt")]
        [InlineData("0009.txt")]
        [InlineData("0010.txt")]
        public void Test1(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var p2 = Path.Combine(OutputDir, fileName);

            var code = File.ReadAllText(p1);
            var goodHTML = File.ReadAllText(p2);

            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());

            Assert.Equal(goodHTML, resultHTML);
        }

        [Fact]
        public void TestOverrideCSS_1()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "";
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(myCSS));

            Assert.DoesNotContain("style", resultHTML);
        }

        [Fact]
        public void TestOverrideCSS_2()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "<style>MY_CSS</style>";
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(myCSS));
            Assert.Contains(myCSS, resultHTML);
        }
    }
}