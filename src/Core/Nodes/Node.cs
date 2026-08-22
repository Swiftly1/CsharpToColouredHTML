namespace CsharpToColouredHTML.Core.Nodes;

internal record Node
{
    private string _Colour = NodeColors.DefaultColour;
    private string _Text = string.Empty;
    private string _Trivia = string.Empty;
    private string _ClassificationType = string.Empty;
    private bool _HasNewLine = false;
    private bool _UsesMostCommonColour = false;
    private bool _AlreadyMarked = false;
    private int _LineNumber = 0;

    private Node()
    {
    }

    public Node(List<Node> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            throw new ArgumentNullException(nameof(nodes));

        if (nodes.Count > 1)
        {
            // Copy Content
            Nodes = new List<Node>(nodes);
        }
        else
        {
            this.Colour = nodes[0]._Colour;
            this.Text = nodes[0].Text;
            this.Trivia = nodes[0].Trivia;
            this.ClassificationType = nodes[0].ClassificationType;
            this.HasNewLine = nodes[0].HasNewLine;
            this.UsesMostCommonColour = nodes[0].UsesMostCommonColour;
            this.AlreadyMarked = nodes[0].AlreadyMarked;
            this.LineNumber = nodes[0].LineNumber;
        }
    }

    public Guid Id = Guid.NewGuid();

    public List<Node> Nodes { get; set; } = new();

    public bool IsChain => Nodes.Count > 1;

    public string ClassificationType
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(ClassificationType)} when there are many nodes is invalid");

            return _ClassificationType;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(ClassificationType)} when there are many nodes is invalid");

            _ClassificationType = value;
        }
    }

    public int LineNumber
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(LineNumber)} when there are many nodes is invalid");

            return _LineNumber;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(LineNumber)} when there are many nodes is invalid");

            _LineNumber = value;
        }
    }

    public bool AlreadyMarked
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(AlreadyMarked)} when there are many nodes is invalid");

            return _AlreadyMarked;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(AlreadyMarked)} when there are many nodes is invalid");

            _AlreadyMarked = value;
        }
    }

    public bool UsesMostCommonColour
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(UsesMostCommonColour)} when there are many nodes is invalid");

            return _UsesMostCommonColour;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(UsesMostCommonColour)} when there are many nodes is invalid");

            _UsesMostCommonColour = value;
        }
    }

    public bool HasNewLine
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(HasNewLine)} when there are many nodes is invalid");

            return _HasNewLine;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(HasNewLine)} when there are many nodes is invalid");

            _HasNewLine = value;
        }
    }

    public string Trivia
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Trivia)} when there are many nodes is invalid");

            return _Trivia;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Trivia)} when there are many nodes is invalid");

            _Trivia = value;
        }
    }

    public string Text
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Text)} when there are many nodes is invalid");

            return _Text;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Text)} when there are many nodes is invalid");

            _Text = value;
        }
    }

    public string Colour
    {
        get
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Colour)} when there are many nodes is invalid");

            return _Colour;
        }
        set
        {
            if (IsChain)
                throw new Exception($"Accessing {nameof(Colour)} when there are many nodes is invalid");

            _Colour = value;
        }
    }

    public static Node CreateNode(string currentClassificationType, string text, string trivia)
    {
        var node = new Node();
        node._ClassificationType = currentClassificationType;
        node._Text = text;
        node._Trivia = trivia;
        node._HasNewLine = (trivia + text).Contains(Environment.NewLine);
        return node;
    }

    public static Node CreateNode(string currentClassificationType, string text, string trivia, bool hasNewLine)
    {
        var node = new Node();
        node._ClassificationType = currentClassificationType;
        node._Text = text;
        node._Trivia = trivia;
        node._HasNewLine = hasNewLine;
        return node;
    }

    public override string ToString()
    {
        if (IsChain)
            return $"Chain: {string.Join(" | ", Nodes.Select(x => x.Text))}";

        return $"\"{_Text}\" is {_ClassificationType}";
    }
}