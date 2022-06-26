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
        [ClassData(typeof(FilesTestData))]
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
        [ClassData(typeof(FilesTestData))]
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

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void WithoutLineNumbersOptimized(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_Optimized."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings().UseCustomCSS("").EnableOptimizations().DisableLineNumbers();
            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void LineNumbersOptimized(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_LineNumbersOptimized."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings().UseCustomCSS("").EnableOptimizations();
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