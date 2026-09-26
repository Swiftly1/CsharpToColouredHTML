using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.StateMachines
{
    internal class CallParametersStateMachine
    {
        public NodeEnumerationHelper Walker { get; }

        public SharedPassContext Context { get; }

        public CallParametersStateMachine(NodeEnumerationHelper walker, SharedPassContext context)
        {
            Walker = walker;
            Context = context;
        }

        public bool WalkOverParams()
        {
            var anyChanged = false;

            if (Walker.CurrentText != "(")
                return false;

            if (!Walker.MoveNext())
                return false;

            var currentState = ParamState.Expression;
            var parenthesisCounter = 1;

            do
            {
                if (Walker.CurrentText == "(")
                    parenthesisCounter++;

                if (Walker.CurrentText == ")")
                    parenthesisCounter--;

                if (parenthesisCounter == 0)
                    break;

                if (currentState == ParamState.Expression)
                {
                    if (Walker.TryPeekAhead(out var next) && next.Text == ":")
                    {
                        currentState = ParamState.NamedParam;
                        Walker.MoveBehind();
                        continue;
                    }

                    var success = false;

                    var typeWalker = new ExpressionWalker(Walker, Context);
                    success = typeWalker.ConsumeExpressionAhead(ExpressionWalkState.Chain, ExpressionWalkMode.Default);

                    if (!success)
                    {
                        Walker.MoveBehind();
                        Logger.Warning("Couldnt handle type correctly for some reason.");
                        break;
                    }

                    anyChanged |= success;
                    currentState = ParamState.CommaOrEnd;
                }
                else if (currentState == ParamState.CommaOrEnd)
                {
                    if (Walker.CurrentNode.IsChain)
                        throw new Exception("Chain shouldn't be on comma position");

                    if (Walker.CurrentText.EqualsAnyOf(",", ")"))
                        currentState = ParamState.Expression;
                    else
                        break;
                }
                else if (currentState == ParamState.NamedParam)
                {
                    Context.MarkNodeAs(Walker.CurrentNode, NodeColors.ParameterName);

                    if (!Walker.MoveNext())
                        break;

                    if (Walker.CurrentText != ":")
                        break;

                    currentState = ParamState.Expression;
                    anyChanged = true;
                }
            } while (Walker.MoveNext());

            return anyChanged;
        }

        private enum ParamState
        {
            Expression,
            CommaOrEnd,
            NamedParam,
        }
    }
}
