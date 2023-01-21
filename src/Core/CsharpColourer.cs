using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Classification;

// Classifier Helpers
using Microsoft.AspNetCore.Mvc;

namespace CsharpToColouredHTML.Core;

public class CsharpColourer
{
    public readonly Hints Hints = new Hints();
    public readonly CsharpColourerSettings Settings = new CsharpColourerSettings();

    public CsharpColourer() { }

    public CsharpColourer(CsharpColourerSettings settings)
    {
        Settings = settings;
    }

    public string ProcessSourceCode(string code, IEmitter emitter)
    {
        code = code.ReplaceLineEndings();
        var nodes = GenerateInternalRepresentation(code);
        var heuristics = new HeuristicsGenerator(Hints).Build(nodes);
        return emitter.Emit(heuristics);
    }

    private List<Node> GenerateInternalRepresentation(string code)
    {
        var nodes = new List<Node>();
        var (spans, srcText) = GetClassifiedSpans(code);

        TextSpan? previous = null;
        var skippedClassifications = new List<string> { ClassificationTypeNames.StringEscapeCharacter };

        for (int i = 0; i < spans.Count; i++)
        {
            var current = spans[i];
            try
            {
                if (skippedClassifications.Contains(current.ClassificationType))
                    continue;

                if (i > 0 && current.ClassificationType == ClassificationTypeNames.StaticSymbol)
                {
                    previous = spans[i - 1].TextSpan;
                    continue;
                }

                var index = previous?.End ?? 0;
                var length = current.TextSpan.Start - index;
                var triviaTextSpan = new TextSpan(index, length);
                var trivia = srcText.GetSubText(triviaTextSpan);

                var node = new Node(current.ClassificationType, srcText.ToString(current.TextSpan), trivia.ToString());

                var crazyStrings = new List<string> { ClassificationTypeNames.VerbatimStringLiteral, ClassificationTypeNames.ExcludedCode };

                if (crazyStrings.Contains(node.ClassificationType) && node.Text.Contains(Environment.NewLine))
                {
                    var splitted = node.Text.Split(Environment.NewLine);

                    for (int y = 0; y < splitted.Length; y++)
                    {
                        var split_node = new Node
                        (
                            node.ClassificationType,
                            splitted[y],
                            y == 0 ? node.Trivia : Environment.NewLine,
                            y == 0 ? node.Trivia.Contains(Environment.NewLine) : true
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

    private static readonly ImmutableArray<MetadataReference> _coreReferences =
    ImmutableArray.Create<MetadataReference>
    (
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
    );

    private static readonly ImmutableArray<MetadataReference> _coreReferencesWithASP =
    ImmutableArray.Create<MetadataReference>
    (
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ControllerBase).Assembly.Location)
    );

    private (List<ClassifiedSpan> ClassifiedSpans, SourceText SourceText) GetClassifiedSpans(string code)
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var sourceText = SourceText.From(code);
        var workspace = new AdhocWorkspace(host);

        var proj = workspace.AddProject("Test", LanguageNames.CSharp);

        // TODO: If possible, then get rid of external ASP nuget.
        if (code.Contains("using Microsoft.AspNetCore") && code.Contains("ControllerBase"))
        {
            proj = proj.AddMetadataReferences(_coreReferencesWithASP);
        }
        else
        {
            proj = proj.AddMetadataReferences(_coreReferences);
        }

        var doc = proj.AddDocument("TestFile", sourceText);

        var spans = Classifier.GetClassifiedSpansAsync(doc, new TextSpan(0, code.Length)).GetAwaiter().GetResult().ToList();
        return (spans, sourceText);
    }
}