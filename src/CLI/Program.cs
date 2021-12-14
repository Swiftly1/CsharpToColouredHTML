using CsharpToColouredHTML.Core;

var code = File.ReadAllText("code.txt");

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());

//Console.WriteLine(html);

File.WriteAllText(@"C:\Users\User\Desktop\test.html", html);

//new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter());