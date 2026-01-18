using CsharpToColouredHTML.Core.HeuristicsGeneration;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.MarkKnownStuff;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.PreFlight;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class PassManager
{
    private List<Pass> _Passes = new();

    public void RegisterPass(Pass pass)
    {
        if (_Passes.Any(x => x.Name == pass.Name))
            throw new Exception($"Cannot register the same pass twice. Pass name: '{pass.Name}'.");

        _Passes.Add(pass);
    }

    public void RunPasses(List<NodeInternalRepresentation> nodes)
    {
        foreach (var pass in _Passes)
        {
            Logger.Info($"Running Pass: '{pass.Name}'");
            pass.Run(nodes);
        }
    }

    internal static PassManager CreateDefault(Hints hints)
    {
        var pm = new PassManager();

        var sharedCxt = new SharedPassContext();
        sharedCxt.Hints = hints;

        pm.RegisterPass(new PreFlightPass(sharedCxt));
        pm.RegisterPass(new MarkKnownStuffPass(sharedCxt));

        return pm;
    }
}
