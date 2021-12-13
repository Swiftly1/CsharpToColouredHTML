using CsharpToColouredHTML.Core;

var code =
@"using MarkdownSharp;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Blog.Controllers;

public class HomeController : Controller
{
    public HomeController()
    {

    }

    public IActionResult Index()
    {
        string contents;
        using (var wc = new System.Net.WebClient())
            contents = wc.DownloadString(""https://raw.githubusercontent.com/asd"");

        var test = new Markdown(new MarkdownOptions { });
        var html = test.Transform(contents);
        return View(""Index"", JsonConvert.SerializeObject(html));
    }
}";

var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());

Console.WriteLine(html);

new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter(false));