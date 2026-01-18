using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.PreFlight;

internal class PreFlightPass : Pass
{
    // The purpose of this pass is to collect as much reliable hints as possible
    // In order to use them in other passes so they can generate better heuristics.

    public override string Name { get => "PreFlightAnalysis"; }

    public PreFlightPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PreFlightResult Run(List<NodeInternalRepresentation> input)
    {
        return new PreFlightResult();
    }
}