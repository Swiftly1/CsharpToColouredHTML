using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.StateMachines;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.RecordsArgs;


internal class RecordsArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "RecordsArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        var flattenNodes = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flattenNodes);

        var workListIndices = ExtractIndicesOfRecords(Walker);

        foreach (var index in workListIndices)
        {
            var asm = new ArgumentStateMachine(Walker, Context);
            var result = asm.WalkOverArgs(index);
        }

        return new PassResult();
    }

    private List<int> ExtractIndicesOfRecords(NodeEnumerationHelper walker)
    {
        var indices = new List<int>();
        do
        {
            if (walker.CurrentNode.IsChain)
                continue;

            if (walker.CC != ClassificationTypeNames.Keyword || walker.CurrentText != "record")
                continue;

            if (!walker.MoveNext())
                continue;

            var isExplicitClassOrStruct = walker.CurrentText.EqualsAnyOf("class", "struct");
            var isImplicitClass = walker.CC == ClassificationTypeNames.ClassName;

            if (isExplicitClassOrStruct)
            {
                if (!walker.MoveNext())
                    continue;

                indices.Add(walker.CurrentIndex);
            }
            else if (isImplicitClass)
            {
                indices.Add(walker.CurrentIndex);
            }
        } while (walker.MoveNext());
        return indices;
    }
}