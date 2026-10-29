using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.CastDetection;

internal class CastDetectionPass : Pass
{
    public override string Name { get => "CastDetection"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public CastDetectionPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            var isValid = Walker.CC.EqualsAnyOf
            (
                ClassificationTypeNames.LocalName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.ConstantName,
                ClassificationTypeNames.ParameterName,
                ClassificationTypeNames.MethodName
            );

            if (!isValid)
                continue;

            // Ensure that pattern is like: (Something)variable
            if (!Walker.TryPeekBehind(out var closingParenthesis, 1) ||
                !Walker.TryPeekBehind(out var type, 2) ||
                !Walker.TryPeekBehind(out var openingParenthesis, 3))
                continue;

            if (closingParenthesis.IsChain || closingParenthesis.Text != ")")
                continue;

            if (openingParenthesis.IsChain || openingParenthesis.Text != "(")
                continue;

            if (type.IsChain)
            {
                var castType = new NodeEnumerationHelper(type.Nodes);
                var typeWalker = new TypeWalker(castType, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
            }
            else
            {
                var indexAfterCast = Walker.CurrentIndex;

                // (TestQ.Home)h;
                //  ^---------- jump here
                Walker.CurrentIndex = Walker.Nodes.IndexOf(type);

                var typeWalker = new TypeWalker(Walker, Context);
                typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);

                // (TestQ.Home)h;
                // ------------^ jump here
                Walker.CurrentIndex = indexAfterCast;
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}