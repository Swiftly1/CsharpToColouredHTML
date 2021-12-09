using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Classification;

namespace Core;
public class CsharpColourer
{
    public void ProcessSourceCode(string code, IEmitter emitter)
    {
        var result = GetClassifiedSpans(code);
        var srcText = result.SourceText;

        TextSpan? previous = null;

        foreach (var current in result.ClassifiedSpans)
        {
            var index = previous?.End ?? 0;
            var length = current.TextSpan.Start - index;
            var triviaTextSpan = new TextSpan(index, length);
            var trivia = srcText.GetSubText(triviaTextSpan);

            emitter.EmitText(trivia.ToString());
            emitter.EmitNode(current, srcText);

            previous = current.TextSpan;
        }
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