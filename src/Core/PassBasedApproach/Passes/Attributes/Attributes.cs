using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Attributes;

internal class AttributesPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "Attributes"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CurrentText != "[")
                continue;

            var invalidClassifications = new[]
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.LocalName,
                ClassificationTypeNames.ConstantName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.ParameterName,
                ClassificationTypeNames.Operator
            };

            if (Walker.TryPeekBehind(out var before) && before.ClassificationType.EqualsAnyOf(invalidClassifications))
                continue;

            if (!Walker.MoveNext())
                continue;

            Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
        } while (Walker.MoveNext());

        return new PassResult();
    }
}