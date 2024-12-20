using CsharpToColouredHTML.Core;

var filePath = "code.txt";

if (args.Length > 0 && args[0].Length > 0)
    filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File '{filePath}' does not exist");
    return;
}

var code = File.ReadAllText(filePath);

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());
File.WriteAllText(@"C:\Users\User\Desktop\file.html", html);
var options = new HTMLEmitter();

int number = 54;
options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = false, Optimize = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesDisabled_OptimizationsDisabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = false, Optimize = true });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesDisabled_OptimizationsEnabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = true, Optimize = false });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesEnabled_OptimizationsDisabled.txt", html);

options = new HTMLEmitter(new HTMLEmitterSettings { AddLineNumber = true, Optimize = true });
html = new CsharpColourer().ProcessSourceCode(code, options);
File.WriteAllText($"{number.ToString().PadLeft(4, '0')}_LinesEnabled_OptimizationsEnabled.txt", html);

Console.WriteLine(html);