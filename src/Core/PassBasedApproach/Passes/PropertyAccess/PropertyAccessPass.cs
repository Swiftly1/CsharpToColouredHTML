using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PropertyAccess;

internal class PropertyAccessPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "PropertyAccess"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        var validCCs = new[]
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.FieldName
        };

        do
        {
            if (!Walker.CC.EqualsAnyOf(validCCs))
                continue;

            if (!Walker.TryPeekAhead(out var dot) || dot.Text != ".")
                continue;

            var chain = new List<NodeInternalRepresentation>
            {
                Walker.CurrentNode
            };

            var propertyAccessChainCCs = new[]
            {
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.Identifier
            };

            while (Walker.MoveNext())
            {
                if (Walker.CC.EqualsAnyOf(propertyAccessChainCCs) || Walker.CurrentText == ".")
                    chain.Add(Walker.CurrentNode);
                else
                    break;
            }

            foreach (var current in chain)
            {
                if (current.Text == ".")
                    Walker.MarkNodeAs(current, NodeColors.Operator);
                else
                    Walker.MarkNodeAs(current, Walker.ResolveVariable(current));
            }

        } while (Walker.MoveNext());

        return new PassResult();
    }
}