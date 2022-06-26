namespace CsharpToColouredHTML.Core
{
    public record NodeWithDetails
    (
        Node Node,
        string Colour,
        bool IsNew,
        bool IsUsing,
        int ParenthesisCounter
    );
}
