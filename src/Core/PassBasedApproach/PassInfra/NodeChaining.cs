using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra
{
    internal class NodeChaining
    {
        public static List<NodeWrapper> ChainNodes(List<Node> nodes)
        {
            var list = new List<NodeWrapper>();

            var validIdentifiers = new string[]
            {
                ClassificationTypeNames.NamespaceName,
                ClassificationTypeNames.LocalName,
                ClassificationTypeNames.FieldName,
                ClassificationTypeNames.PropertyName,
                ClassificationTypeNames.Identifier,
                ClassificationTypeNames.ConstantName,
                ClassificationTypeNames.ParameterName,
                ClassificationTypeNames.MethodName,

            };

            for (int i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];
            }

            return list;
        }
    }
}
