using CsharpToColouredHTML.Core.Miscs;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.VariableAssignment;

internal class VariableAssignmentPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "VariableAssignment"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (!Walker.CurrentText.EqualsAnyOf("="))
                continue;

            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentText.EqualsAnyOf("new"))
                continue;

            Console.WriteLine(Walker.CurrentNode.IsChain);
            Console.WriteLine();
        } while (Walker.MoveNext());
        return new PassResult();
    }
}