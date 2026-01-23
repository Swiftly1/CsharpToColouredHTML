using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class NamespacesPass : Pass
{
    public override string Name { get => "Namespaces"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public NamespacesPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (Walker.CurrentText != "using")
                continue;

            const int IDENTIFIER = 0;
            const int OPERATOR = 1;
            var currentState = IDENTIFIER;

            while (Walker.MoveNext())
            {
                if (currentState == IDENTIFIER)
                {
                    if (!Walker.CC.EqualsAnyOf(ClassificationTypeNames.NamespaceName, ClassificationTypeNames.Identifier))
                    {
                        Walker.MoveBehind();
                        break;
                    }

                    Walker.MarkNodeAs(NodeColors.Namespace);
                    currentState = OPERATOR;
                }
                else if (currentState == OPERATOR)
                {
                    if (Walker.CurrentText != ".")
                    {
                        Walker.MoveBehind();
                        break;
                    }

                    Walker.MarkNodeAs(NodeColors.Operator);
                    currentState = IDENTIFIER;
                }
                else
                {
                    break;
                }
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }
}