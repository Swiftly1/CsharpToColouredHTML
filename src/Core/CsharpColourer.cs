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
        var (Spans, srcText) = GetClassifiedSpans(code);

        TextSpan? previous = null;
        var skippedClassifications = new List<string> { ClassificationTypeNames.StringEscapeCharacter };

        for (int i = 0; i < Spans.Count; i++)
        {
            var current = Spans[i];
            try
            {
                if (skippedClassifications.Contains(current.ClassificationType))
                    continue;

                if (i > 0 && current.ClassificationType == ClassificationTypeNames.StaticSymbol)
                {
                    previous = Spans[i - 1].TextSpan;
                    continue;
                }

                var index = previous?.End ?? 0;
                var length = current.TextSpan.Start - index;
                var triviaTextSpan = new TextSpan(index, length);
                var trivia = srcText.GetSubText(triviaTextSpan);

                var node = new Node(current.ClassificationType, srcText.ToString(current.TextSpan), trivia.ToString());

                if (node.ClassificationType == ClassificationTypeNames.VerbatimStringLiteral &&
                    node.Text.Contains(Environment.NewLine))
                {
                    var splitted = node.Text.Split(Environment.NewLine);

                    for (int y = 0; y < splitted.Length; y++)
                    {
                        var split_node = new Node
                        (
                            node.ClassificationType,
                            splitted[y],
                            y == 0 ? node.Trivia : Environment.NewLine,
                            true,
                            1
                        );

                        nodes.Add(split_node);
                    }
                }
                else
                {
                    nodes.Add(node);
                }

                previous = current.TextSpan;
            }
            catch
            {
                var node = new Node(current.ClassificationType, srcText.ToString(current.TextSpan), "");
                nodes.Add(node);
            }
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