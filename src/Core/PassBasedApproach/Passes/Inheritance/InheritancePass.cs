using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Inheritance;

internal class InheritancePass : Pass
{
    public override string Name { get => "Inheritance"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public InheritancePass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CC != ClassificationTypeNames.Keyword)
                continue;

            if (!Walker.CurrentText.EqualsAnyOf("class", "struct"))
                continue;

            // Indices:
            // 0,       1,                  2,      3
            // class    HomeController      :       Controller
            // If we cannot have 3, then skip 
            if (!Walker.TryPeekAhead(out var _, 3))
                continue;

            if (!Walker.TryPeekAhead(out var identifier, 1))
                continue;

            var validClassifications = new string[]
            {
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.RecordClassName,
                ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.InterfaceName,
                ClassificationTypeNames.Identifier,
            };

            if (!identifier.ClassificationType.EqualsAnyOf(validClassifications))
                continue;

            if (!Walker.TryPeekAhead(out var inheritance, 2) || inheritance.Text != ":")
                continue;

            Walker.MoveNext(3);

            const int IDENTIFIER = 0;
            const int COMMA = 1;
            var currentState = IDENTIFIER;

            do
            {
                if (currentState == IDENTIFIER)
                {
                    Walker.MarkLastElementOfChainAsClassOrStruct(Walker.CurrentNode);

                    currentState = COMMA;
                }
                else
                {
                    if (Walker.CurrentText == ",")
                    {
                        currentState = IDENTIFIER;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            } while (Walker.MoveNext());
        } while (Walker.MoveNext());

        return new PassResult();
    }
}