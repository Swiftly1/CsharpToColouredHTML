using CsharpToColouredHTML.Core;
using CsharpToColouredHTML.Core.Emitters.HTML;

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

// Or:
//new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter()); 

Console.WriteLine(html);