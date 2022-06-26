using Xunit;
using System;
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
        [InlineData("0011.txt")]
        [InlineData("0012.txt")]
        [InlineData("0013.txt")]
        [InlineData("0014.txt")]
        [InlineData("0015.txt")]
        [InlineData("0016.txt")]
        [InlineData("0017.txt")]
        [InlineData("0018.txt")]
        [InlineData("0019.txt")]
        [InlineData("0020.txt")]
        [InlineData("0021.txt")]
        [InlineData("0022.txt")]
        [InlineData("0023.txt")]
        public void WithoutLineNumbers(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var p2 = Path.Combine(OutputDir, fileName);

            var code = File.ReadAllText(p1);
            var goodHTML = File.ReadAllText(p2);

            var settings = new HTMLEmitterSettings().UseCustomCSS("").DisableLineNumbers().DisableOptimizations();
            var emitter = new HTMLEmitter(settings);
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(goodHTML, resultHTML);
        }

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
        [InlineData("0011.txt")]
        [InlineData("0012.txt")]
        [InlineData("0013.txt")]
        [InlineData("0014.txt")]
        [InlineData("0015.txt")]
        [InlineData("0016.txt")]
        [InlineData("0017.txt")]
        [InlineData("0018.txt")]
        [InlineData("0019.txt")]
        [InlineData("0020.txt")]
        [InlineData("0021.txt")]
        [InlineData("0022.txt")]
        [InlineData("0023.txt")]
        public void LineNumbers(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_LineNumbers."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings().UseCustomCSS("").DisableOptimizations();
            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void TestOverrideCSS_1()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "";
            var settings = new HTMLEmitterSettings().UseCustomCSS(myCSS).DisableLineNumbers().DisableOptimizations();
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));

            Assert.DoesNotContain("style", resultHTML);
        }

        [Fact]
        public void TestOverrideCSS_2()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "<style>MY_CSS</style>";
            var settings = new HTMLEmitterSettings().UseCustomCSS(myCSS);
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
            Assert.Contains(myCSS, resultHTML);
        }

        [Fact]
        public void AllIndicesOfTest()
        {
            var str = $"test {Environment.NewLine}{Environment.NewLine} test";

            Assert.Equal(2, StringHelper.AllIndicesOf(str, Environment.NewLine).Count);
        }
    }
}