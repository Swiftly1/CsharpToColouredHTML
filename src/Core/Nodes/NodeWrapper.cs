namespace CsharpToColouredHTML.Core.Nodes;

internal record NodeWrapper
{
    public Guid Id = Guid.NewGuid();

    public NodeWrapper(Node node)
    {
        Node = node;
    }

    public NodeWrapper(List<Node> node)
    {
        // Copy Content
        Nodes = new List<Node>(node);
    }

    public List<Node> Nodes { get; set; } = new();

    public bool IsChain => Nodes.Count > 1;

    public Node Node
    {
        get
        {
            if (IsChain)
            {
                throw new Exception("Accessing .Node when there are many nodes is invalid");
            }

            return Nodes[0];
        }
        private set
        {
            Nodes = new List<Node> { value };
        }
    }

    public string Text
    {
        get
        {
            if (IsChain)
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
            if (IsChain)
            {
                throw new Exception("Accessing .ClassificationType when there are many nodes is invalid");
            }

            return Node.ClassificationType;
        }
    }

    public override string ToString()
    {
        return $"Text? '{(IsChain ? "Chain" : Text)}' CC '{(IsChain ? "Chain" : ClassificationType)}' Colour '{(IsChain ? "Chain" : Node.Colour)}'";
    }
}