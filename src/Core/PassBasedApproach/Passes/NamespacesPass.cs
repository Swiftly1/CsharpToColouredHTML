using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.Functions;

internal class NamespacesPass : Pass
{
    public override string Name { get => "Namespaces"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public NamespacesPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        do
        {
            if (Walker.CurrentNode.IsChain)
                continue;

            if (Walker.CurrentText == "using")
            {
                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Keyword);

                if (Walker.TryPeekAhead(out var assignment, 2) && !assignment.IsChain && assignment.Text == "=")
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
                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Keyword);

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
                    Context.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Context.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Context.MarkNodeAs(chain, NodeColors.Namespace);
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

        Context.MarkNodeAs(name, NodeColors.Class);
        Context.MarkNodeAs(assignment, NodeColors.Punctuation);

        if (chain.IsChain)
        {
            foreach (var ns in chain.Nodes)
            {
                if (ns.ClassificationType == ClassificationTypeNames.Operator)
                {
                    Context.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Context.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Context.MarkNodeAs(name, NodeColors.Namespace);
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
                    Context.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Context.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Context.MarkNodeAs(name, NodeColors.Namespace);
        }
    }
}