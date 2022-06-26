namespace CsharpToColouredHTML.Core
{
    public record NodeWithDetails
    (
        string Colour,
        string Text,
        string Trivia,
        string TextWithTrivia,
        bool HasNewLine,
        bool IsNew,
        bool IsUsing,
        int ParenthesisCounter
    );
}
