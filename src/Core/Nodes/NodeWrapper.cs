namespace CsharpToColouredHTML.Core.Nodes;

internal record NodeWrapper
{
    public List<NodeInternalRepresentation> Nodes { get; set; } = new();

    public bool IsChain => Nodes.Count > 1;

    public NodeInternalRepresentation Node
    {
        get
        {
            if (Nodes.Count > 1)
            {
                throw new Exception("Accessing .Node when there are many nodes is invalid");
            }

            return Nodes[0];
        }
    }

    public Guid Id
    {
        get
        {
            if (Nodes.Count > 1)
            {
                throw new Exception("Accessing .Id when there are many nodes is invalid");
            }

            return Node.Id;
        }
    }

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