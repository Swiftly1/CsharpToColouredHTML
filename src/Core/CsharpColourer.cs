using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core;

public class CsharpColourer
{
    public string ProcessSourceCode(string code, IEmitter emitter)
    {
        var nodes = GenerateInternalRepresentation(code);
        emitter.Emit(nodes);
        return emitter.Text;
    }

    private List<Node> GenerateInternalRepresentation(string code)
    {
        var nodes = new List<Node>();
        var result = GetClassifiedSpans(code);
        var srcText = result.SourceText;

        TextSpan? previous = null;

        foreach (var current in result.ClassifiedSpans)
        {
            var index = previous?.End ?? 0;
            var length = current.TextSpan.Start - index;
            var triviaTextSpan = new TextSpan(index, length);
            var trivia = srcText.GetSubText(triviaTextSpan);

            var node = new Node(current.ClassificationType, srcText.ToString(current.TextSpan), trivia.ToString());
            nodes.Add(node);

            previous = current.TextSpan;
        }

        return nodes;
    }

    private (List<ClassifiedSpan> ClassifiedSpans, SourceText SourceText) GetClassifiedSpans(string code)
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var workspace = new AdhocWorkspace(host);
        var sourceText = SourceText.From(code);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);

        var compilation = CSharpCompilation
                          .Create("Test")
                          .AddReferences(MetadataReference
                          .CreateFromFile(typeof(object).Assembly.Location))
                          .AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return (Classifier.GetClassifiedSpans(semanticModel, new TextSpan(0, code.Length), workspace).ToList(), sourceText);
    }
}