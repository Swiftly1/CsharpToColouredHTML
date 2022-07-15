# CsharpToColouredHTML

This library tries to convert C# code into pure HTML with some lines of CSS that

make it look as if that was inside Visual Studio or Visual Studio Code.

# Why? 

Motivation for doing it was that I wanted to put C# code fragments on website, 

but I didn't like the colours that were provided by popular syntax highlighting solutions - they were not so IDE-ish, 

but also I wanted to have server-side rendering, so users aren't required to have JavaScript enabled* in order to see colors.

\* Ironically demo page uses a few lines of js just to make messing with the demo more sane

# Security Considerations

Even despite performing [escaping](https://github.com/Swiftly1/CsharpToColouredHTML/blob/master/src/Core/HTML/HTMLEmitter.cs#L32)

I still recommend to use it only on trusted inputs, at least for now. 

# Demo 

You can try live demo that's avaliable at: https://csharp-colors.xyz/

Or use it in not so serious projects: https://www.nuget.org/packages/CsharpToColouredHTML.Core/

.NET CLI: `dotnet add package CsharpToColouredHTML.Core --version 1.0.22`

Sample Usage:

	var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());
	Console.WriteLine(html);

```diff
- ⚠️ If you want to have pure HTML without Iframe and HTML being escaped, then try this:
```
	var settings = new HTMLEmitterSettings().DisableIframe();
	var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
	Console.WriteLine(html);

___

Custom CSS:

	var myCustomCSS = "<style>...</style>";
	var settings = new HTMLEmitterSettings().UseCustomCSS(myCSS);
	var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
	Console.WriteLine(html);

# [CSS Customizability](https://github.com/Swiftly1/CsharpToColouredHTML/blob/master/Customizability.md)

# [Change Log](https://github.com/Swiftly1/CsharpToColouredHTML/blob/master/CHANGELOG.md)

# Examples

* [Example 1 - Code from this project](#ex1)

* [Example 2 - Random code from Roslyn](#ex2)

Example input (`code.txt`):

    using System.Text;
    using Microsoft.CodeAnalysis.Classification;

    namespace CsharpToColouredHTML.Core;

    public class ConsoleEmitter : IEmitter
    {
        public ConsoleEmitter(bool addDiagnosticInfo = false)
        {
            this.addDiagnosticInfo = addDiagnosticInfo;
        }

        private readonly StringBuilder _sb = new StringBuilder();

        private readonly bool addDiagnosticInfo;

        public string Text { get; private set; }

        public void Emit(List<Node> nodes)
        {
            Console.ResetColor();

            Text = "";
            _sb.Clear();

            foreach (var node in nodes)
            {
                EmitNode(node);
            }

            Text = _sb.ToString();
        }

        public void EmitNode(Node node)
        {
            if (node.ClassificationType == ClassificationTypeNames.ClassName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (node.ClassificationType == ClassificationTypeNames.NamespaceName)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (node.ClassificationType == ClassificationTypeNames.Identifier)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (node.ClassificationType == ClassificationTypeNames.Keyword)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (node.ClassificationType == ClassificationTypeNames.LocalName)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (node.ClassificationType == ClassificationTypeNames.MethodName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (node.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (node.ClassificationType == ClassificationTypeNames.Operator)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (node.ClassificationType == ClassificationTypeNames.ControlKeyword)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (node.ClassificationType == ClassificationTypeNames.VerbatimStringLiteral)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else if (node.ClassificationType == ClassificationTypeNames.ParameterName)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ResetColor();
            }

            _sb.Append(node.TextWithTrivia);
            Console.Write($"{node.TextWithTrivia}");

            if (addDiagnosticInfo)
            {
                _sb.Append($"({node.ClassificationType})");
                Console.Write($"({node.ClassificationType})");
            }

            Console.ResetColor();
        }
    }

<a name="ex1">How generated HTML looks in browser:</a>

![obraz](https://user-images.githubusercontent.com/77643169/149753179-aeb199fb-a83c-4436-ad21-bbacebfc9cb7.png)

Generated HTML Code:

	<style>
	.background{font-family:monaco,Consolas,LucidaConsole,monospace;background-color:#1E1E1E;overflow:scroll;}.numeric{color:#b5cea8;}.method{color:#DCDCAA;}.class{color:#4EC9B0;}.keyword{color:#569cd6;}.blue{color:#9CDCFE;}.white{color:#D4D4D4;}.string{color:#ce9178;}.interface{color:#b8d7a3;}.control{color:#C586C0;}.internal_error{color:#FF0D0D;}.comment{color:#6A9955;}.preprocessor{color:#808080;}.preprocessor_text{color:#a4a4a4;}.struct{color:#86C691;}
	table{color:white;white-space:pre;}.line_no::before{content:attr(line_no);}.code_column{padding-left:5px;}
	</style>
	<pre class="background">
	<table>
	<tbody>
	<tr><td class="line_no" line_no="0"></td><td class="code_column"><span class="keyword">using</span> <span class="white">System</span><span class="white">.</span><span class="white">Text</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="1"></td><td class="code_column"><span class="keyword">using</span> <span class="white">Microsoft</span><span class="white">.</span><span class="white">CodeAnalysis</span><span class="white">.</span><span class="white">Classification</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="2"></td><td></tr><tr><td class="line_no" line_no="3"></td><td class="code_column"><span class="keyword">namespace</span> <span class="white">CsharpToColouredHTML</span><span class="white">.</span><span class="white">Core</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="4"></td><td></tr><tr><td class="line_no" line_no="5"></td><td class="code_column"><span class="keyword">public</span> <span class="keyword">class</span> <span class="class">ConsoleEmitter</span> <span class="white">:</span> <span class="interface">IEmitter</span></td></tr><tr><td class="line_no" line_no="6"></td><td class="code_column"><span class="white">{</span></td></tr><tr><td class="line_no" line_no="7"></td><td class="code_column">    <span class="keyword">public</span> <span class="class">ConsoleEmitter</span><span class="white">(</span><span class="keyword">bool</span> <span class="blue">addDiagnosticInfo</span> <span class="white">=</span> <span class="keyword">false</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="8"></td><td class="code_column">    <span class="white">{</span></td></tr><tr><td class="line_no" line_no="9"></td><td class="code_column">        <span class="keyword">this</span><span class="white">.</span><span class="white">addDiagnosticInfo</span> <span class="white">=</span> <span class="blue">addDiagnosticInfo</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="10"></td><td class="code_column">    <span class="white">}</span></td></tr><tr><td class="line_no" line_no="11"></td><td></tr><tr><td class="line_no" line_no="12"></td><td class="code_column">    <span class="keyword">private</span> <span class="keyword">readonly</span> <span class="class">StringBuilder</span> <span class="white">_sb</span> <span class="white">=</span> <span class="keyword">new</span> <span class="class">StringBuilder</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="13"></td><td></tr><tr><td class="line_no" line_no="14"></td><td class="code_column">    <span class="keyword">private</span> <span class="keyword">readonly</span> <span class="keyword">bool</span> <span class="white">addDiagnosticInfo</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="15"></td><td></tr><tr><td class="line_no" line_no="16"></td><td class="code_column">    <span class="keyword">public</span> <span class="keyword">string</span> <span class="white">Text</span> <span class="white">{</span> <span class="keyword">get</span><span class="white">;</span> <span class="keyword">private</span> <span class="keyword">set</span><span class="white">;</span> <span class="white">}</span></td></tr><tr><td class="line_no" line_no="17"></td><td></tr><tr><td class="line_no" line_no="18"></td><td class="code_column">    <span class="keyword">public</span> <span class="keyword">void</span> <span class="method">Emit</span><span class="white">(</span><span class="class">List</span><span class="white">&lt;</span><span class="class">Node</span><span class="white">&gt;</span> <span class="blue">nodes</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="19"></td><td class="code_column">    <span class="white">{</span></td></tr><tr><td class="line_no" line_no="20"></td><td class="code_column">        <span class="class">Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="21"></td><td class="code_column">        <span class="white">Text</span> <span class="white">=</span> <span class="string">&quot;&quot;</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="22"></td><td class="code_column">        <span class="white">_sb</span><span class="white">.</span><span class="method">Clear</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="23"></td><td></tr><tr><td class="line_no" line_no="24"></td><td class="code_column">        <span class="control">foreach</span> <span class="white">(</span><span class="keyword">var</span> <span class="blue">node</span> <span class="control">in</span> <span class="blue">nodes</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="25"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="26"></td><td class="code_column">            <span class="method">EmitNode</span><span class="white">(</span><span class="blue">node</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="27"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="28"></td><td></tr><tr><td class="line_no" line_no="29"></td><td class="code_column">        <span class="white">Text</span> <span class="white">=</span> <span class="white">_sb</span><span class="white">.</span><span class="method">ToString</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="30"></td><td class="code_column">    <span class="white">}</span></td></tr><tr><td class="line_no" line_no="31"></td><td></tr><tr><td class="line_no" line_no="32"></td><td class="code_column">    <span class="keyword">public</span> <span class="keyword">void</span> <span class="method">EmitNode</span><span class="white">(</span><span class="class">Node</span> <span class="blue">node</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="33"></td><td class="code_column">    <span class="white">{</span></td></tr><tr><td class="line_no" line_no="34"></td><td class="code_column">        <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">ClassName</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="35"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="36"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Red</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="37"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="38"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">NamespaceName</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="39"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="40"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Green</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="41"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="42"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">Identifier</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="43"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="44"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Green</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="45"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="46"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">Keyword</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="47"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="48"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Yellow</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="49"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="50"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">StringLiteral</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="51"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="52"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Cyan</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="53"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="54"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">LocalName</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="55"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="56"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Blue</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="57"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="58"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">MethodName</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="59"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="60"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Red</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="61"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="62"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">Punctuation</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="63"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="64"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">White</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="65"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="66"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">Operator</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="67"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="68"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">White</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="69"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="70"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">ControlKeyword</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="71"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="72"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">DarkRed</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="73"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="74"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">VerbatimStringLiteral</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="75"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="76"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">DarkRed</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="77"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="78"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">StringLiteral</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="79"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="80"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">DarkGreen</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="81"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="82"></td><td class="code_column">        <span class="control">else</span> <span class="control">if</span> <span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span> <span class="white">==</span> <span class="class">ClassificationTypeNames</span><span class="white">.</span><span class="white">ParameterName</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="83"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="84"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="white">ForegroundColor</span> <span class="white">=</span> <span class="class">ConsoleColor</span><span class="white">.</span><span class="white">Yellow</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="85"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="86"></td><td class="code_column">        <span class="control">else</span></td></tr><tr><td class="line_no" line_no="87"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="88"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="89"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="90"></td><td></tr><tr><td class="line_no" line_no="91"></td><td class="code_column">        <span class="white">_sb</span><span class="white">.</span><span class="method">Append</span><span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">TextWithTrivia</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="92"></td><td class="code_column">        <span class="class">Console</span><span class="white">.</span><span class="method">Write</span><span class="white">(</span><span class="string">$&quot;</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">TextWithTrivia</span><span class="white">}</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="93"></td><td></tr><tr><td class="line_no" line_no="94"></td><td class="code_column">        <span class="control">if</span> <span class="white">(</span><span class="white">addDiagnosticInfo</span><span class="white">)</span></td></tr><tr><td class="line_no" line_no="95"></td><td class="code_column">        <span class="white">{</span></td></tr><tr><td class="line_no" line_no="96"></td><td class="code_column">            <span class="white">_sb</span><span class="white">.</span><span class="method">Append</span><span class="white">(</span><span class="string">$&quot;</span><span class="string">(</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white">}</span><span class="string">)</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="97"></td><td class="code_column">            <span class="class">Console</span><span class="white">.</span><span class="method">Write</span><span class="white">(</span><span class="string">$&quot;</span><span class="string">(</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white">}</span><span class="string">)</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="98"></td><td class="code_column">        <span class="white">}</span></td></tr><tr><td class="line_no" line_no="99"></td><td></tr><tr><td class="line_no" line_no="100"></td><td class="code_column">        <span class="class">Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span></td></tr><tr><td class="line_no" line_no="101"></td><td class="code_column">    <span class="white">}</span></td></tr><tr><td class="line_no" line_no="102"></td><td class="code_column"><span class="white">}</span></td></tr></tbody>
	</table></pre>


Second Example:

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    #nullable disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Task = System.Threading.Tasks.Task;

    namespace Roslyn.Compilers.Extension
    {
        [Guid("31C0675E-87A4-4061-A0DD-A4E510FCCF97")]
        public sealed class CompilerPackage : AsyncPackage
        {
            public static string RoslynHive = null;

            protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
            {
                await base.InitializeAsync(cancellationToken, progress).ConfigureAwait(true);

                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                var reg = (ILocalRegistry2)await GetServiceAsync(typeof(SLocalRegistry)).ConfigureAwait(true);
                cancellationToken.ThrowIfCancellationRequested();
                Assumes.Present(reg);

                var packagePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string localRegistryRoot;
                reg.GetLocalRegistryRoot(out localRegistryRoot);
                var registryParts = localRegistryRoot.Split('\\');

                // Is it a valid Hive looks similar to:  
                //  'Software\Microsoft\VisualStudio\14.0'  'Software\Microsoft\VisualStudio\14.0Roslyn'  'Software\Microsoft\VSWinExpress\14.0'
                if (registryParts.Length >= 4)
                {
                    var skuName = registryParts[2];
                    var hiveName = registryParts[3];
                    RoslynHive = string.Format(@"{0}.{1}", registryParts[2], registryParts[3]);

                    await WriteMSBuildFilesAsync(packagePath, RoslynHive, cancellationToken).ConfigureAwait(true);

                    try
                    {
                        Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.DisableMarkDirty = true;
                        Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.SetGlobalProperty("RoslynHive", RoslynHive);
                    }
                    finally
                    {
                        Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.DisableMarkDirty = false;
                    }
                }
            }

            private async Task WriteMSBuildFilesAsync(string packagePath, string hiveName, CancellationToken cancellationToken)
            {
                // A map of the file name to the content we need to ensure exists in the file
                var filesToWrite = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // The props we want to be included as early as possible since we want our tasks to be used and
                // to ensure our setting of targets path happens early enough
                filesToWrite.Add(await GetMSBuildRelativePathAsync($@"Imports\Microsoft.Common.props\ImportBefore\Roslyn.Compilers.Extension.{hiveName}.props", cancellationToken).ConfigureAwait(true),
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>
    <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
      <PropertyGroup Condition=""'$(RoslynHive)' == '{hiveName}'"">
        <CSharpCoreTargetsPath>{packagePath}\Microsoft.CSharp.Core.targets</CSharpCoreTargetsPath>
        <VisualBasicCoreTargetsPath>{packagePath}\Microsoft.VisualBasic.Core.targets</VisualBasicCoreTargetsPath>
      </PropertyGroup> 

      <UsingTask TaskName=""Microsoft.CodeAnalysis.BuildTasks.Csc"" AssemblyFile=""{packagePath}\Microsoft.Build.Tasks.CodeAnalysis.dll"" Condition=""'$(RoslynHive)' == '{hiveName}'"" />
      <UsingTask TaskName=""Microsoft.CodeAnalysis.BuildTasks.Vbc"" AssemblyFile=""{packagePath}\Microsoft.Build.Tasks.CodeAnalysis.dll"" Condition=""'$(RoslynHive)' == '{hiveName}'"" />
    </Project>");

                // This targets content we want to be included later since the project file might touch UseSharedCompilation
                var targetsContent =
                        $@"<?xml version=""1.0"" encoding=""utf-8""?>
    <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
      <!-- If we're not using the compiler server, set ToolPath/Exe to direct to the exes in this package -->
      <PropertyGroup Condition=""'$(RoslynHive)' == '{hiveName}' and '$(UseSharedCompilation)' == 'false'"">
        <CscToolPath>{packagePath}</CscToolPath>
        <CscToolExe>csc.exe</CscToolExe>
        <VbcToolPath>{packagePath}</VbcToolPath>
        <VbcToolExe>vbc.exe</VbcToolExe>
      </PropertyGroup>
    </Project>";

                filesToWrite.Add(await GetMSBuildRelativePathAsync($@"Microsoft.CSharp.targets\ImportBefore\Roslyn.Compilers.Extension.{hiveName}.targets", cancellationToken).ConfigureAwait(true), targetsContent);
                filesToWrite.Add(await GetMSBuildRelativePathAsync($@"Microsoft.VisualBasic.targets\ImportBefore\Roslyn.Compilers.Extension.{hiveName}.targets", cancellationToken).ConfigureAwait(true), targetsContent);

                // First we want to ensure any Roslyn files with our hive name that we aren't writing -- this is probably
                // leftovers from older extensions
                var msbuildDirectory = new DirectoryInfo(await GetMSBuildPathAsync(cancellationToken).ConfigureAwait(true));
                if (msbuildDirectory.Exists)
                {
                    foreach (var file in msbuildDirectory.EnumerateFiles($"*Roslyn*{hiveName}*", SearchOption.AllDirectories))
                    {
                        if (!filesToWrite.ContainsKey(file.FullName))
                        {
                            file.Delete();
                        }
                    }
                }

                try
                {
                    foreach (var fileAndContents in filesToWrite)
                    {
                        var parentDirectory = new DirectoryInfo(Path.GetDirectoryName(fileAndContents.Key));
                        parentDirectory.Create();

                        // If we already know the file has the same contents, then we can skip
                        if (File.Exists(fileAndContents.Key) && File.ReadAllText(fileAndContents.Key) == fileAndContents.Value)
                        {
                            continue;
                        }

                        File.WriteAllText(fileAndContents.Key, fileAndContents.Value);
                    }
                }
                catch (Exception e)
                {
                    var msg =
    $@"{e.Message}

    To reload the Roslyn compiler package, close Visual Studio and any MSBuild processes, then restart Visual Studio.";

                    VsShellUtilities.ShowMessageBox(
                        this,
                        msg,
                        null,
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }


            private async Task<string> GetMSBuildVersionStringAsync(CancellationToken cancellationToken)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                var dte = (DTE)await GetServiceAsync(typeof(SDTE)).ConfigureAwait(true);
                var parts = dte.Version.Split('.');
                if (parts.Length != 2)
                {
                    throw new Exception($"Unrecognized Visual Studio Version: {dte.Version}");
                }

                int majorVersion = int.Parse(parts[0]);

                if (majorVersion >= 16)
                {
                    // Starting in Visual Studio 2019, the folder is just called "Current". See
                    // https://github.com/Microsoft/msbuild/issues/4149 for further commentary.
                    return "Current";
                }
                else
                {
                    return majorVersion + ".0";
                }
            }

            private async Task<string> GetMSBuildPathAsync(CancellationToken cancellationToken)
            {
                var version = await GetMSBuildVersionStringAsync(cancellationToken).ConfigureAwait(true);
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, $@"Microsoft\MSBuild\{version}");
            }

            private async Task<string> GetMSBuildRelativePathAsync(string relativePath, CancellationToken cancellationToken)
            {
                return Path.Combine(await GetMSBuildPathAsync(cancellationToken).ConfigureAwait(true), relativePath);
            }
        }
    }

<a name="ex2">Generated HTML inside Browser:</a>

![obraz](https://user-images.githubusercontent.com/77643169/146063126-88da0555-f268-4d8e-9b23-47ce59a9b062.png)

