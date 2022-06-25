using CsharpToColouredHTML.Core;

namespace CsharpToColouredHTML
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
