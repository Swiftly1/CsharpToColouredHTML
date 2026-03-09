using CsharpToColouredHTML.Core;
using CsharpToColouredHTML.Core.Emitters.HTML;
#pragma warning disable CS0162

var filePath = "code.txt";

if (args.Length > 0 && args[0].Length > 0)
    filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File '{filePath}' does not exist");
    return;
}

var code = File.ReadAllText(filePath);
var settings = new HTMLEmitterSettings
{
    UseIframe = false,
    Optimize = false,
    AddLineNumber = false
};

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
File.WriteAllText(@"C:\Users\User\Desktop\test.html", html);
var options = new HTMLEmitter();
return;
int number = 84;
options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = false, Optimize = false, UserProvidedCSS = "", UseIframe = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesDisabled_OptimizationsDisabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = false, Optimize = true, UserProvidedCSS = "", UseIframe = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesDisabled_OptimizationsEnabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = true, Optimize = false, UserProvidedCSS = "", UseIframe = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesEnabled_OptimizationsDisabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = true, Optimize = true, UserProvidedCSS = "", UseIframe = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesEnabled_OptimizationsEnabled.txt", html);

Console.WriteLine(html);

