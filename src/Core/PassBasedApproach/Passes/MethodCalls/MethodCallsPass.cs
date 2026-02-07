using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.MethodCalls;

internal class MethodCallsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "MethodCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            var validClassifications = new string[]
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.MethodName,
            };

            if (!Walker.CC.EqualsAnyOf(validClassifications))
                continue;

            if (!Walker.TryPeekAhead(out var parenthesis) || parenthesis.Text != "(")
                continue;

        } while (Walker.MoveNext());

        return new PassResult();
    }
}