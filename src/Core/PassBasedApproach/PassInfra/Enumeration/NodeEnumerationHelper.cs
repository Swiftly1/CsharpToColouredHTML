using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;


internal class NodeEnumerationHelper : EnumerationHelper<Node>
{
    public NodeEnumerationHelper(List<Node> nodes) : base(nodes)
    {

    }

    public string CurrentText => Nodes[CurrentIndex].IsChain ? "Chain" : Nodes[CurrentIndex].Text;

    // Current Classification - "CC" in short because it is used very often.
    public string CC => Nodes[CurrentIndex].IsChain ? "Chain" : Nodes[CurrentIndex].ClassificationType;
}
