using CsharpToColouredHTML.Core;

var code = File.ReadAllText("code.txt");

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());

File.WriteAllText("output.html", html);

//new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter());