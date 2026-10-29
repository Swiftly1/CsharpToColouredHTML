using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.TypeOf;

internal class TypeOfPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "TypeOf"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (Walker.CurrentText != "typeof")
                continue;

            // Skip '('
            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentNode.IsChain || Walker.CurrentText != "(")
                continue;

            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentNode.IsChain)
            {
                var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                var typeWalker = new TypeWalker(exprEnumeration, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }
            else
            {
                var typeWalker = new TypeWalker(Walker, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}