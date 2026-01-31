using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.MethodCalls;

internal class MethodCallsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "MethodCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            var validClassifications = new string[]
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.MethodName,
            };

            if (!Walker.CC.EqualsAnyOf(validClassifications))
                continue;

            if (!Walker.TryPeekAhead(out var parenthesis) && parenthesis.Text == "(")
                continue;

        } while (Walker.MoveNext());

        return new PassResult();
    }
}