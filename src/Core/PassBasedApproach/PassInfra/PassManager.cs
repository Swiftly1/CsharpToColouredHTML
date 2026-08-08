using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.Attributes;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.CasualExpressions;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionArgs;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionType;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.Inheritance;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.MarkKnownStuff;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.MethodCalls;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.ObjectInitializer;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.PreFlight;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.PrintNodes;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.PropertyAccess;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.ReturnType;
using CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

internal class PassManager
{
    private readonly List<Pass> _Passes = new();
    private readonly SharedPassContext SharedPassContext = new();

    public void RegisterPass(Pass pass)
    {
        if (_Passes.Any(x => x.Name == pass.Name))
            throw new Exception($"Cannot register the same pass twice. Pass name: '{pass.Name}'.");

        _Passes.Add(pass);
    }

    public void RunPasses(List<Node> nodes)
    {
        foreach (var pass in _Passes)
        {
            Logger.PrintFancy("Running Pass: '", pass.Name, "'", ConsoleColor.Green);

            var result = pass.Run(nodes);

            if (!result.Success)
            {
                Logger.Error($"Failed at '{pass.Name}' pass. Quiting.");
                return;
            }

            PrintFoundStuff();
        }
    }

    private void PrintFoundStuff()
    {
        if (SharedPassContext.FunctionDeclarationLocations.Any())
        {
            Logger.Info("Found Methods:");

            foreach (var entry in SharedPassContext.FunctionDeclarationLocations)
            {
                Logger.Info(entry.FunctionName, 2);
            }
        }

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

        // Debug
        pm.RegisterPass(new PrintNodesPass(pm.SharedPassContext));

        // Fundamental Passes
        pm.RegisterPass(new MarkKnownStuffPass(pm.SharedPassContext));
        pm.RegisterPass(new PreFlightPass(pm.SharedPassContext));

        // Normal Passes
        pm.RegisterPass(new NamespacesPass(pm.SharedPassContext));
        pm.RegisterPass(new AttributesPass(pm.SharedPassContext));
        pm.RegisterPass(new InheritancePass(pm.SharedPassContext));
        pm.RegisterPass(new ClassMemberTypePass(pm.SharedPassContext));
        pm.RegisterPass(new NewInstancesPass(pm.SharedPassContext));
        pm.RegisterPass(new GenericsMarkerPass(pm.SharedPassContext));
        pm.RegisterPass(new ReturnTypePass(pm.SharedPassContext));
        pm.RegisterPass(new MethodCallsPass(pm.SharedPassContext));
        pm.RegisterPass(new IfStatementPass(pm.SharedPassContext));
        pm.RegisterPass(new UsingInstructionPass(pm.SharedPassContext));
        pm.RegisterPass(new VariableAssignmentPass(pm.SharedPassContext));
        pm.RegisterPass(new FunctionArgsPass(pm.SharedPassContext));
        pm.RegisterPass(new PropertyAccessPass(pm.SharedPassContext));
        pm.RegisterPass(new ObjectInitializerPass(pm.SharedPassContext));
        pm.RegisterPass(new CasualExpressionsPass(pm.SharedPassContext));
        pm.RegisterPass(new FixDoubleLocalNamePass(pm.SharedPassContext));

        return pm;
    }
}
