using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PreFlight;

internal class PreFlightPass : Pass
{
    // The purpose of this pass is to collect as much reliable hints as possible
    // In order to use them in other passes so they can generate better heuristics.

    public override string Name { get => "PreFlightAnalysis"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public PreFlightPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);
        do
        {
            var isClass  = Walker.CurrentText == "class";
            var isStruct = Walker.CurrentText == "struct";
            var isInterface = Walker.CurrentText == "interface";

            if (!isClass && !isStruct && !isInterface)
                continue;

            if (!Walker.TryPeekAhead(out var identifier))
                continue;

            var validClassifications = new string[]
            {
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.RecordClassName,
                ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.InterfaceName,
                ClassificationTypeNames.Identifier
            };

            if (!identifier.ClassificationType.EqualsAnyOf(validClassifications))
                continue;

            var colour = NodeColors.DefaultColour;

            if (isClass)
                colour = NodeColors.Class;

            if (isStruct)
                colour = NodeColors.Struct;

            if (isInterface)
                colour = NodeColors.Interface;

            Walker.MarkNodeAs(identifier, colour);
        } while (Walker.MoveNext());

        return new PassResult();
    }
}