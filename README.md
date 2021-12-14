# CsharpToColouredHTML

This library tries to convert C# code into pure HTML with some CSS lines 

that looks as if that was inside Visual Studio Code / Studio Studio

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

How generated HTML looks in browser:

![obraz](https://user-images.githubusercontent.com/77643169/146056444-a0fed8fd-b86d-4777-83ff-38363b99f937.png)

Generated HTML Code:

	this is html page
	<br>

	<style>
	.background{font-family:monaco,Consolas,LucidaConsole,monospace;background-color:#1E1E1E;}.numeric{color:#b5cea8;}.method{color:#DCDCAA;}.class{color:#4EC9B0;}.keyword{color:#569cd6;}.blue{color:#9CDCFE;}.white{color:#D4D4D4;}.string{color:#ce9178;}.interface{color:#b8d7a3;}.control{color:#C586C0;}.internal_error{color:#FF0D0D;}
	</style>
	<pre class="background">
	<span class="keyword">using</span><span class="white"> System</span><span class="white">.</span><span class="white">Text</span><span class="white">;</span><span class="keyword">
	using</span><span class="white"> Microsoft</span><span class="white">.</span><span class="white">CodeAnalysis</span><span class="white">.</span><span class="white">Classification</span><span class="white">;</span><span class="keyword">

	namespace</span><span class="white"> CsharpToColouredHTML</span><span class="white">.</span><span class="white">Core</span><span class="white">;</span><span class="keyword">

	public</span><span class="keyword"> class</span><span class="class"> ConsoleEmitter</span><span class="white"> :</span><span class="interface"> IEmitter</span><span class="white">
	{</span><span class="keyword">
		public</span><span class="class"> ConsoleEmitter</span><span class="white">(</span><span class="keyword">bool</span><span class="blue"> addDiagnosticInfo</span><span class="white"> =</span><span class="keyword"> false</span><span class="white">)</span><span class="white">
		{</span><span class="keyword">
			this</span><span class="white">.</span><span class="white">addDiagnosticInfo</span><span class="white"> =</span><span class="blue"> addDiagnosticInfo</span><span class="white">;</span><span class="white">
		}</span><span class="keyword">

		private</span><span class="keyword"> readonly</span><span class="class"> StringBuilder</span><span class="white"> _sb</span><span class="white"> =</span><span class="keyword"> new</span><span class="class"> StringBuilder</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="keyword">

		private</span><span class="keyword"> readonly</span><span class="keyword"> bool</span><span class="white"> addDiagnosticInfo</span><span class="white">;</span><span class="keyword">

		public</span><span class="keyword"> string</span><span class="white"> Text</span><span class="white"> {</span><span class="keyword"> get</span><span class="white">;</span><span class="keyword"> private</span><span class="keyword"> set</span><span class="white">;</span><span class="white"> }</span><span class="keyword">

		public</span><span class="keyword"> void</span><span class="method"> Emit</span><span class="white">(</span><span class="class">List</span><span class="white">&lt;</span><span class="class">Node</span><span class="white">&gt;</span><span class="blue"> nodes</span><span class="white">)</span><span class="white">
		{</span><span class="class">
			Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="white">

			Text</span><span class="white"> =</span><span class="string"> &quot;&quot;</span><span class="white">;</span><span class="white">
			_sb</span><span class="white">.</span><span class="method">Clear</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="control">

			foreach</span><span class="white"> (</span><span class="keyword">var</span><span class="blue"> node</span><span class="control"> in</span><span class="blue"> nodes</span><span class="white">)</span><span class="white">
			{</span><span class="method">
				EmitNode</span><span class="white">(</span><span class="blue">node</span><span class="white">)</span><span class="white">;</span><span class="white">
			}</span><span class="white">

			Text</span><span class="white"> =</span><span class="white"> _sb</span><span class="white">.</span><span class="method">ToString</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="white">
		}</span><span class="keyword">

		public</span><span class="keyword"> void</span><span class="method"> EmitNode</span><span class="white">(</span><span class="class">Node</span><span class="blue"> node</span><span class="white">)</span><span class="white">
		{</span><span class="control">
			if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">ClassName</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Red</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">NamespaceName</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Green</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">Identifier</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Green</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">Keyword</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Yellow</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">StringLiteral</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Cyan</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">LocalName</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Blue</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">MethodName</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Red</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">Punctuation</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">White</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">Operator</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">White</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">ControlKeyword</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">DarkRed</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">VerbatimStringLiteral</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">DarkRed</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">StringLiteral</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">DarkGreen</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="control"> if</span><span class="white"> (</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white"> ==</span><span class="class"> ClassificationTypeNames</span><span class="white">.</span><span class="white">ParameterName</span><span class="white">)</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="white">ForegroundColor</span><span class="white"> =</span><span class="class"> ConsoleColor</span><span class="white">.</span><span class="white">Yellow</span><span class="white">;</span><span class="white">
			}</span><span class="control">
			else</span><span class="white">
			{</span><span class="class">
				Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="white">
			}</span><span class="white">

			_sb</span><span class="white">.</span><span class="method">Append</span><span class="white">(</span><span class="blue">node</span><span class="white">.</span><span class="white">TextWithTrivia</span><span class="white">)</span><span class="white">;</span><span class="class">
			Console</span><span class="white">.</span><span class="method">Write</span><span class="white">(</span><span class="string">$&quot;</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">TextWithTrivia</span><span class="white">}</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span><span class="control">

			if</span><span class="white"> (</span><span class="white">addDiagnosticInfo</span><span class="white">)</span><span class="white">
			{</span><span class="white">
				_sb</span><span class="white">.</span><span class="method">Append</span><span class="white">(</span><span class="string">$&quot;</span><span class="string">(</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white">}</span><span class="string">)</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span><span class="class">
				Console</span><span class="white">.</span><span class="method">Write</span><span class="white">(</span><span class="string">$&quot;</span><span class="string">(</span><span class="white">{</span><span class="blue">node</span><span class="white">.</span><span class="white">ClassificationType</span><span class="white">}</span><span class="string">)</span><span class="string">&quot;</span><span class="white">)</span><span class="white">;</span><span class="white">
			}</span><span class="class">

			Console</span><span class="white">.</span><span class="method">ResetColor</span><span class="white">(</span><span class="white">)</span><span class="white">;</span><span class="white">
		}</span><span class="white">
	}</span></pre>

