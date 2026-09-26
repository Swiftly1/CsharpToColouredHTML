using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.StateMachines;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.MethodCalls;

internal class MethodCallsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "MethodCalls"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        do
        {
            var validClassifications = new string[]
            {
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.MethodName
            };

            if (!Walker.CC.EqualsAnyOf(validClassifications))
                continue;

            if (!Walker.TryPeekAhead(out var parenthesis) || parenthesis.Text != "(")
                continue;

            // public Form1()
            if (Walker.TryPeekBehind(out var previous) && NameResolver.AccessibilityModifiers.Contains(previous.Text))
            {
                // ctor
                var colour = Context.NameResolver.ResolveClassOrStructName(previous);
                Context.MarkNodeAs(Walker.CurrentNode, colour, true);
            }
            else
            {
                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Method, true);

                // If this is function declaration, not a call then skip
                if (Context.FunctionDeclarationLocations.Any(x => x.Index == Walker.CurrentIndex))
                    continue;

                if (!Walker.MoveNext() || Walker.CurrentText != "(")
                    continue;

                var stateMachine = new CallParametersStateMachine(Walker, Context);
                var result = stateMachine.WalkOverParams();
            }

        } while (Walker.MoveNext());

        return new PassResult();
    }
}