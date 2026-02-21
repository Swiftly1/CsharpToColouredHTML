using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionType;

internal class ClassMemberTypePass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "ClassMemberType"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CurrentNode.IsChain)
            {
                continue;
            }
            else
            {
                var isValid = Walker.CurrentText.EqualsAnyOf(NameResolver.CommonKeywordsBeforeTypeName);

                if (!isValid)
                    continue;
            }

            var validClassifications = new[]
            {
                ClassificationTypeNames.MethodName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.DelegateName
            };

            //   \/            \/
            // public void EmitNode(Node node)
            var funcNameIndex = Walker.CurrentIndex + 2;
            if (!(Walker.TryPeekAhead(out var methodName, 2) && !methodName.IsChain && methodName.ClassificationType.EqualsAnyOf(validClassifications)))
                continue;

            var isFuncCall = Walker.TryPeekAhead(out var parenthesis, 3) && parenthesis.Text == "(";

            if (Walker.TryPeekAhead(out var type))
            {
                var success = false;

                if (type.IsChain)
                {
                    var enumeration = new NodeEnumerationHelper(type.Nodes);
                    var typeWalker = new TypeWalker(enumeration, Context);
                    success = typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
                }
                else
                {
                    Walker.MoveNext();
                    var typeWalker = new TypeWalker(Walker, Context);
                    success =typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);
                }

                if (success && isFuncCall)
                {
                    Context.FunctionDeclarationLocations.Add((methodName.Text, funcNameIndex));
                }
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}