# CsharpToColouredHTML

Converting C# source code into coloured HTML(todo) or just printing it into the console.

Example:

    using Core;

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

    new CsharpColourer().ProcessSourceCode(code, new ConsoleEmitter());
    
![img](https://user-images.githubusercontent.com/77643169/145472450-b8db4510-4560-4ec0-a340-d04532a6d545.png)

