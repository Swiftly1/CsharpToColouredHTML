using CsharpToColouredHTML.Core;

var code = File.ReadAllText("code.txt");

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());

Console.WriteLine(html);

new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter());