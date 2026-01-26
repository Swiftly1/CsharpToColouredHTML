using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.FunctionArgs;

internal class FunctionArgsPass(SharedPassContext ctx) : Pass(ctx)
{
    public override string Name { get => "FunctionArgs"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public override PassResult Run(List<NodeInternalRepresentation> input)
    {
        Walker = new NodeEnumerationHelper(input, Context);

        foreach (var function in Context.FunctionDeclarationLocations)
        {
            Logger.Info($"Function Location: {function.FunctionName} at {function.Index}");

            Walker.CurrentIndex = function.Index;

            if (!Walker.MoveNext() || Walker.CurrentText != "(")
                continue;

            var currentState = ArgsWalkState.Type;
            var parenthesisCounter = 1;

            while (Walker.MoveNext())
            {
                if (Walker.CurrentText == "(")
                    parenthesisCounter++;

                if (Walker.CurrentText == ")")
                    parenthesisCounter--;

                if (parenthesisCounter <= 0 || Walker.CurrentText.EqualsAnyOf("{", ";"))
                    break;

                if (currentState == ArgsWalkState.Type)
                {
                    if (!Walker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType))
                        break;

                    currentState = ArgsWalkState.Identifier;
                }
                else if (currentState == ArgsWalkState.Identifier)
                {
                    Walker.MarkNodeAs(NodeColors.ParameterName);
                    currentState = ArgsWalkState.CommaOrEnd;
                }
                else if (currentState == ArgsWalkState.CommaOrEnd)
                {
                    if (Walker.CurrentText.EqualsAnyOf(","))
                    {
                        currentState = ArgsWalkState.Type;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    throw new NotImplementedException("State not handled");
                }
            }
        }

        return new PassResult();
    }

    public enum ArgsWalkState
    {
        Type,
        Identifier,
        CommaOrEnd
    }
}