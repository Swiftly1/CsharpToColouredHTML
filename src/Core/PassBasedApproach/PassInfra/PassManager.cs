using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.Inheritance;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.MarkKnownStuff;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.PreFlight;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class PassManager
{
    private List<Pass> _Passes = new();
    private SharedPassContext SharedPassContext = new();

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
            Logger.PrintFancy("Running Pass: '", pass.Name, "'", ConsoleColor.Green);
            pass.Run(nodes);

            PrintFoundStuff();
        }
    }

    private void PrintFoundStuff()
    {
        if (SharedPassContext.FoundClasses.Any())
        {
            Logger.Info("Found Classes:");

            foreach (var entry in SharedPassContext.FoundClasses)
            {
                Logger.Info(entry, 2);
            }
        }

        if (SharedPassContext.FoundStructs.Any())
        {
            Logger.Info("Found Structs:");

            foreach (var entry in SharedPassContext.FoundStructs)
            {
                Logger.Info(entry, 2);
            }
        }

        if (SharedPassContext.FoundInterfaces.Any())
        {
            Logger.Info("Found Interfaces:");

            foreach (var entry in SharedPassContext.FoundInterfaces)
            {
                Logger.Info(entry, 2);
            }
        }

        Logger.Info("");
        Logger.Info("");
    }

    internal static PassManager CreateDefault(Hints hints)
    {
        var pm = new PassManager();

        pm.SharedPassContext.Hints = hints;

        // Order - unfortunately, matters :(
        pm.RegisterPass(new PreFlightPass(pm.SharedPassContext));
        pm.RegisterPass(new MarkKnownStuffPass(pm.SharedPassContext));
        pm.RegisterPass(new InheritancePass(pm.SharedPassContext));

        return pm;
    }
}
