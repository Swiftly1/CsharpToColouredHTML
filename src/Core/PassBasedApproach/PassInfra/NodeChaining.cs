using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra
{
    internal static class NodeChaining
    {
        public static List<NodeWrapper> ChainNodes(List<Node> nodes)
        {
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
                ClassificationTypeNames.NamespaceName,
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordClassName,
                ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.InterfaceName,
                ClassificationTypeNames.TypeParameterName
            };

            var output = new List<NodeWrapper>();
            var chain = new List<Node>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];

                var isChain = current.ClassificationType.EqualsAnyOf(validIdentifiers)
                    || current.Text == ".";

                if (isChain)
                {
                    chain.Add(current);
                }
                else
                {
                    if (chain.Count >= 1)
                    {
                        output.Add(new NodeWrapper(chain));
                        chain.Clear();
                    }

                    output.Add(new NodeWrapper(current));
                }
            }

            if (chain.Any())
                output.Add(new NodeWrapper(chain));

            return output;
        }

        internal static List<NodeWrapper> FlattenNodes(List<NodeWrapper> chained)
        {
            Logger.Info($"Flattening Nodes, before count: {chained.Count()}");

            var output = new List<NodeWrapper>();

            foreach (var entry in chained)
            {
                if (entry.IsChain)
                {
                    foreach (var item in entry.Nodes)
                    {
                        output.Add(new NodeWrapper(item));
                    }
                }
                else
                {
                    output.Add(new NodeWrapper(entry.Node));
                }
            }

            Logger.Info($"Flattening Nodes, after count: {output.Count()}");
            return output;
        }
    }
}
