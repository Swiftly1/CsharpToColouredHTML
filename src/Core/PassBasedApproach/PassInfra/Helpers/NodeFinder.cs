using CsharpToColouredHTML.Core.Nodes;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers
{
    internal class NodeFinder
    {
        public static int FindNodeInChainedList(Guid id, List<Node> chainedList)
        {
            var currentNodeIndexInChainedList = chainedList.FindIndex(x => x.Id == id);

            if (currentNodeIndexInChainedList == -1)
            {
                for (int i = 0; i < chainedList.Count; i++)
                {
                    var entry = chainedList[i];
                    if (entry.IsChain && entry.Nodes.Any(x => x.Id == id))
                    {
                        currentNodeIndexInChainedList = i;
                        break;
                    }
                }
            }

            return currentNodeIndexInChainedList;
        }
    }
}
