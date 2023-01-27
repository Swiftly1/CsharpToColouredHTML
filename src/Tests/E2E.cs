using Xunit;
using System;
using System.IO;
using CsharpToColouredHTML.Core;
using System.Collections.Generic;

namespace Tests
{
    public class E2E
    {
        private const string InputDir = $"InputCsharp";
        private const string OutputDir = $"OutputHtml";

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void LinesDisabled_OptimizationsDisabled(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var p2 = Path.Combine(OutputDir, fileName.Replace(".", "_LinesDisabled_OptimizationsDisabled."));

            var code = File.ReadAllText(p1);
            var goodHTML = File.ReadAllText(p2);

            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS("")
                               .DisableLineNumbers()
                               .DisableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(goodHTML, resultHTML);
        }

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void LinesEnabled_OptimizationsDisabled(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_LinesEnabled_OptimizationsDisabled."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS("")
                               .EnableLineNumbers()
                               .DisableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void LinesDisabled_OptimizationsEnabled(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_LinesDisabled_OptimizationsEnabled."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS("")
                               .DisableLineNumbers()
                               .EnableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Theory]
        [ClassData(typeof(FilesTestData))]
        public void LinesEnabled_OptimizationsEnabled(string fileName)
        {
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_LinesEnabled_OptimizationsEnabled."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS("")
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void TestOverrideCSS_1()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "";
            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS(myCSS)
                               .DisableLineNumbers()
                               .DisableOptimizations();

            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));

            Assert.DoesNotContain("style", resultHTML);
        }

        [Fact]
        public void TestOverrideCSS_2()
        {
            var code = "Console.WriteLine(\"asd\")";
            var myCSS = "<style>MY_CSS</style>";
            var settings = new HTMLEmitterSettings().UseCustomCSS(myCSS).DisableIframe();
            var resultHTML = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
            Assert.Contains(myCSS, resultHTML);
        }

        [Fact]
        public void AllIndicesOfTest()
        {
            var str = $"test {Environment.NewLine}{Environment.NewLine} test";

            Assert.Equal(2, StringHelper.AllIndicesOf(str, Environment.NewLine).Count);
        }

        [Fact]
        public void CSS_OnlyUsedColours_1()
        {
            var fileName = "0011.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_CSS_LinesEnabled_OptimizationsEnabled."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void CSS_AllColours_1()
        {
            var fileName = "0011.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_CSS."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .EnableLineNumbers()
                               .DisableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void CSS_OnlyUsedColours_2()
        {
            var fileName = "0018.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_CSS_LinesEnabled_OptimizationsEnabled."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void CSS_AllColours_2()
        {
            var fileName = "0018.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, fileName.Replace(".", "_CSS."));
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .EnableLineNumbers()
                               .DisableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void Iframe()
        {
            var fileName = "0027.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, "0027_Iframe.txt");
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .EnableIframe();

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void ConvertEndings()
        {
            var other_new_line_ending = Environment.NewLine == "\r\n" ? "\n" : Environment.NewLine;

            var code_with_other_endings =
                $"public{other_new_line_ending}static{other_new_line_ending}void{other_new_line_ending}Main()";

            var settings = new HTMLEmitterSettings()
                               .UseCustomCSS("")
                               .EnableLineNumbers()
                               .DisableOptimizations()
                               .DisableIframe();

            var emitter = new HTMLEmitter(settings);

            var result = new CsharpColourer().ProcessSourceCode(code_with_other_endings, emitter);

            Assert.Contains("line_no=\"3\"", result);
        }

        [Fact]
        public void TestExternalPostProcessing()
        {
            var linesPath = Path.Combine(OutputDir, "ExternalPostProcessing.txt");
            var lines = File.ReadAllText(linesPath);

            var code = "Console.OnBecameInvisible(123)";

            var htmlSettings = new HTMLEmitterSettings().UseCustomCSS(string.Empty).DisableIframe().DisableOptimizations();

            Action<List<NodeAfterProcessing>> myAction = (List<NodeAfterProcessing> nodes) =>
            {
                var list = new List<string>
                {
                    "OnBecameInvisible"
                };

                foreach (var node in nodes)
                {
                    if (list.Contains(node.Text))
                    {
                        node.Colour = NodeColors.Keyword;
                    }
                    else
                    {
                        node.Colour = NodeColors.Control;
                    }
                }
            };

            var colourerSettings = new CsharpColourerSettings
            {
                PostProcessingAction = myAction
            };

            var resultHTML = new CsharpColourer(colourerSettings).ProcessSourceCode(code, new HTMLEmitter(htmlSettings));
            Assert.Equal(lines, resultHTML);
        }

        [Fact]
        public void Highlight_Predicate()
        {
            var fileName = "0019.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, "0019_Highlighting_Predicate.txt");
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .DisableIframe()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .HighlightThoseLines(x => x != 0 && x % 3 == 0)
                               .UseCustomCSS("");

            var emitter = new HTMLEmitter(settings);
            var linesResult = new CsharpColourer().ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void Highlight_Postprocess()
        {
            var fileName = "0019.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, "0019_Highlighting_Postprocess.txt");
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .DisableIframe()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .UseCustomCSS("");

            var emitter = new HTMLEmitter(settings);
            var colourerSettings = new CsharpColourerSettings
            {
                PostProcessingAction = (list) =>
                {
                    foreach (var item in list)
                        if (item.Text == "WriteLine")
                            item.UseHighlighting = true;
                }
            };

            var linesResult = new CsharpColourer(colourerSettings).ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }

        [Fact]
        public void Highlight_Predicate_And_Postprocess()
        {
            var fileName = "0019.txt";
            var p1 = Path.Combine(InputDir, fileName);
            var code = File.ReadAllText(p1);

            var linesPath = Path.Combine(OutputDir, "0019_Highlighting_PredicateAndPostprocess.txt");
            var p2Lines = File.ReadAllText(linesPath);

            var settings = new HTMLEmitterSettings()
                               .DisableIframe()
                               .EnableLineNumbers()
                               .EnableOptimizations()
                               .HighlightThoseLines(x => x != 0 && x % 3 == 0)
                               .UseCustomCSS("");

            var emitter = new HTMLEmitter(settings);
            var colourerSettings = new CsharpColourerSettings
            {
                PostProcessingAction = (list) =>
                {
                    foreach (var item in list)
                        if (item.Text == "WriteLine")
                            item.UseHighlighting = true;
                }
            };

            var linesResult = new CsharpColourer(colourerSettings).ProcessSourceCode(code, emitter);

            Assert.Equal(p2Lines, linesResult);
        }
    }
}