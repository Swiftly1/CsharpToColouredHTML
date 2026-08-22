using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.StateMachines
{
    internal class ArgumentStateMachine
    {
        public NodeEnumerationHelper Walker { get; }

        public SharedPassContext Context { get; }

        public ArgumentStateMachine(NodeEnumerationHelper walker, SharedPassContext context)
        {
            Walker = walker;
            Context = context;
        }

        public bool WalkOverArgs(int index)
        {
            var anyChanged = false;

            Walker.CurrentIndex = index;

            if (!Walker.MoveNext())
                return false;

            if (Walker.CurrentText != "(")
                return false;

            if (!Walker.MoveNext())
                return false;

            var currentState = ArgsState.Type;
            var parenthesisCounter = 1;

            do
            {
                if (Walker.CurrentText == "(")
                    parenthesisCounter++;

                if (Walker.CurrentText == ")")
                    parenthesisCounter--;

                if (parenthesisCounter == 0)
                    break;

                if (currentState == ArgsState.Type)
                {
                    var success = false;

                    var typeWalker = new TypeWalker(Walker, Context);
                    success = typeWalker.ConsumeTypeAhead(TypeWalkState.TypeName, TypeWalkMode.MustBeType);

                    if (!success)
                        Logger.Warning("Couldnt handle type correctly for some reason.");

                    anyChanged |= success;
                    currentState = ArgsState.Identifier;
                }
                else if (currentState == ArgsState.Identifier)
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
                            anyChanged = true;
                        }
                    }

                    currentState = ArgsState.CommaOrEnd;
                }
                else if (currentState == ArgsState.CommaOrEnd)
                {
                    if (Walker.CurrentNode.IsChain)
                        throw new Exception("Chain shouldn't be on comma position");
                    else
                    {
                        if (Walker.CurrentText.EqualsAnyOf(",", ")"))
                            currentState = ArgsState.Type;
                    }
                }
            } while (Walker.MoveNext());

            return anyChanged;
        }
        private enum ArgsState
        {
            Type,
            Identifier,
            CommaOrEnd
        }
    }
}
