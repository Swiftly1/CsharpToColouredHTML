using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionArgs;

internal class FunctionArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<Node> input)
    {
        Walker = new NodeEnumerationHelper(input);

        foreach (var function in Context.FunctionDeclarationLocations)
        {
            Walker.CurrentIndex = function.Index;

            if (!Walker.MoveNext())
                continue;

            if (Walker.CurrentText != "(")
                continue;

            if (!Walker.MoveNext())
                continue;

            var currentState = FunctionArgsState.Type;
            var parenthesisCounter = 1;

            do
            {
                if (Walker.CurrentText == "(")
                    parenthesisCounter++;

                if (Walker.CurrentText == ")")
                    parenthesisCounter--;

                if (parenthesisCounter == 0)
                    break;

                if (currentState == FunctionArgsState.Type)
                {
                    var success = false;
                    if (Walker.CurrentNode.IsChain)
                    {
                        var exprEnumeration = new NodeEnumerationHelper(Walker.CurrentNode.Nodes);
                        var expressionWalker = new ExpressionWalker(exprEnumeration, Context);
                        success = expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
                    }
                    else
                    {
                        var expressionWalker = new ExpressionWalker(Walker, Context);
                        success = expressionWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);
                    }

                    if (!success)
                        Logger.Warning("Couldnt handle type correctly for some reason.");

                    currentState = FunctionArgsState.Identifier;
                }
                else if (currentState == FunctionArgsState.Identifier)
                {
                    var validCCs = new[]
                    {
                        ClassificationTypeNames.Identifier,
                        ClassificationTypeNames.ParameterName,
                    };

                    if (Walker.CurrentNode.IsChain)
                        throw new Exception("Chain shouldn't be on param name position");
                    else
                    {
                        if (Walker.CC.EqualsAnyOf(validCCs))
                        {
                            Context.MarkNodeAs(Walker.CurrentNode, NodeColors.ParameterName);
                        }
                    }

                    currentState = FunctionArgsState.CommaOrEnd;
                }
                else if (currentState == FunctionArgsState.CommaOrEnd)
                {
                    if (Walker.CurrentNode.IsChain)
                        throw new Exception("Chain shouldn't be on comma position");
                    else
                    {
                        if (Walker.CurrentText.EqualsAnyOf(",", ")"))
                            currentState = FunctionArgsState.Type;
                    }
                }
            } while (Walker.MoveNext());
        }

        return new PassResult();
    }

    private enum FunctionArgsState
    {
        Type,
        Identifier,
        CommaOrEnd
    }
}