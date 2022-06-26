﻿namespace CsharpToColouredHTML.Core;

internal record NodeWithDetails
{
    public NodeWithDetails(string colour, string text, string trivia, bool hasNewLine, bool isNew, bool isUsing, int parenthesisCounter)
    {
        Colour = colour;
        Text = text;
        Trivia = trivia;
        HasNewLine = hasNewLine;
        IsNew = isNew;
        IsUsing = isUsing;
        ParenthesisCounter = parenthesisCounter;
        TextWithTrivia = Trivia + Text;
    }

    public string Colour { get; init; }

    public string Text { get; init; }

    public string Trivia { get; init; }

    public string TextWithTrivia { get; init; }

    public bool HasNewLine { get; init; }

    public bool IsNew { get; init; }

    public bool IsUsing { get; init; }

    public int ParenthesisCounter { get; init; }

    public bool UsesMostCommonColour { get; set; }
}