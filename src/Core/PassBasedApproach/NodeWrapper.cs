using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach;

internal record NodeWrapper
{
    public bool IsChain => Nodes.Count > 1;

    public NodeInternalRepresentation Node => Nodes[0];

    public List<NodeInternalRepresentation> Nodes { get; set; } = new();

    public string Text
    {
        get
        {
            if (Nodes.Count > 1)
            {
                throw new Exception("Accessing .Text when there are many nodes is invalid");
            }
            return Node.Text;
        }
    }

    public string ClassificationType
    {
        get
        {
            if (Nodes.Count > 1)
            {
                throw new Exception("Accessing .ClassificationType when there are many nodes is invalid");
            }

            return Node.ClassificationType;
        }
    }
}