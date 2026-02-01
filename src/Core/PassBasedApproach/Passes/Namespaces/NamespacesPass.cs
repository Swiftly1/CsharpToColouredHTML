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

    public override PassResult Run(List<NodeWrapper> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        do
        {
            if (Walker.CurrentText == "using")
            {
                Walker.MarkNodeAs(NodeColors.Keyword);

                if (Walker.TryPeekAhead(out var assignment, 2) && assignment.Text == "=")
                {
                    HandleNamespaceAlias();
                }
                else
                {
                    HandleNormalNameSpaces();
                }
            }
            else if (Walker.CurrentText == "namespace")
            {
                Walker.MarkNodeAs(NodeColors.Keyword);

                HandleNamespaceDeclaration();
            }
        } while (Walker.MoveNext());

        return new PassResult();
    }

    private void HandleNamespaceDeclaration()
    {
        if (!Walker!.TryPeekAhead(out var chain))
            return;

        if (chain.IsChain)
        {
            foreach (var ns in chain.Nodes)
            {
                if (ns.ClassificationType == ClassificationTypeNames.Operator)
                {
                    Walker.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Walker.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Walker.MarkNodeAs(chain, NodeColors.Namespace);
        }
    }

    private void HandleNamespaceAlias()
    {
        if (!Walker!.TryPeekAhead(out var name))
            return;

        if (!Walker.TryPeekAhead(out var assignment, 2))
            return;

        if (!Walker.TryPeekAhead(out var chain, 3))
            return;

        Walker.MarkNodeAs(name.Node, NodeColors.Class);
        Walker.MarkNodeAs(assignment.Node, NodeColors.Punctuation);

        if (chain.IsChain)
        {
            foreach (var ns in chain.Nodes)
            {
                if (ns.ClassificationType == ClassificationTypeNames.Operator)
                {
                    Walker.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Walker.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Walker.MarkNodeAs(name.Node, NodeColors.Namespace);
        }
    }

    private void HandleNormalNameSpaces()
    {
        if (!Walker!.TryPeekAhead(out var name))
            return;

        if (name.IsChain)
        {
            foreach (var ns in name.Nodes)
            {
                if (ns.ClassificationType == ClassificationTypeNames.Operator)
                {
                    Walker.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Walker.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Walker.MarkNodeAs(name, NodeColors.Namespace);
        }
    }
}