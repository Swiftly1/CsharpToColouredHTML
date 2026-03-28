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
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            var isValid = Walker.CurrentText.EqualsAnyOf(NameResolver.CommonKeywordsBeforeTypeName);

            if (!isValid)
                continue;

            if (Walker.TryPeekAhead(out var next_keyword))
            {
                if (next_keyword.Text.EqualsAnyOf(NameResolver.CommonKeywordsBeforeTypeName))
                    continue;
            }

            var validClassifications = new[]
            {
                ClassificationTypeNames.MethodName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.DelegateName
            };

            var funcNameIndex = Walker.CurrentIndex + 2;
            var originalIndex = Walker.CurrentIndex;
            Node methodName = null;

            // public Abc<int, (float, double)> Index2()
            do
            {
                if (Walker.CC.EqualsAnyOf(validClassifications))
                {
                    funcNameIndex = Walker.CurrentIndex;
                    methodName = Walker.CurrentNode;
                    break;
                }

                if (Walker.CurrentText.EqualsAnyOf(";", "{"))
                {
                    Walker.CurrentIndex = originalIndex;
                    break;
                }
            } while (Walker.MoveNext());

            if (methodName == null)
                continue;
            else
                Walker.CurrentIndex = originalIndex;


            var isFuncCall = Walker.TryPeekAtIndex(out var parenthesis, funcNameIndex + 1) && parenthesis.Text == "(";

            if (Walker.TryPeekAhead(out var type))
            {
                var success = false;

                Walker.MoveNext();
                var typeWalker = new TypeWalker(Walker, Context);
                success = typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);

                if (success && isFuncCall)
                {
                    Context.FunctionDeclarationLocations.Add((methodName.Text, funcNameIndex));
                }
            }

        } while (Walker.MoveNext());
        return new PassResult();
    }
}