using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra
{
    internal class NodeChaining
    {
        public static List<Node> ChainNodes(List<Node> nodes, Hints hints)
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
                ClassificationTypeNames.TypeParameterName,
                ClassificationTypeNames.StringLiteral,
            };

            var output = new List<Node>();
            var chain = new List<Node>();

            var state = 0;
            var genericsCounter = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];

                var isChain = false;
                var chainMustStop = false;

                if (state == 0)
                {
                    isChain = current.ClassificationType.EqualsAnyOf(validIdentifiers);

                    isChain |= current.Text.EqualsAnyOf(hints.BuiltInTypes.ToArray());

                    if (isChain)
                        state = 1;
                }
                else
                {
                    isChain = current.Text == ".";
                    var isPunc = current.ClassificationType == ClassificationTypeNames.Punctuation;

                    if (isPunc)
                    {
                        isChain |= current.Text.EqualsAnyOf("(", "[", "]");

                        if (current.Text.Equals("<"))
                        {
                            genericsCounter++;
                            isChain = true;
                        }
                        else if (current.Text.Equals(">"))
                        {
                            genericsCounter--;
                            isChain = true;
                        }

                        if (genericsCounter > 0 && current.Text.Equals(","))
                        {
                            isChain = true;
                        }
                    }

                    if (current.ClassificationType == ClassificationTypeNames.Keyword)
                        isChain |= current.Text.EqualsAnyOf(hints.BuiltInTypes.ToArray());

                    if (current.Text.EqualsAnyOf("]"))
                        chainMustStop = true;

                    if (current.ClassificationType == ClassificationTypeNames.Punctuation && current.Text.EqualsAnyOf(">", "["))
                    {
                        state = 1;
                    }
                    else
                    {
                        state = 0;
                    }
                }

                if (isChain)
                {
                    chain.Add(current);
                }
                else
                {
                    if (chain.Count >= 1)
                    {
                        output.Add(new Node(chain));
                        chain.Clear();
                    }

                    output.Add(current);
                }

                if (chainMustStop)
                {
                    output.Add(new Node(chain));
                    chain.Clear();
                    state = 0;
                    continue;
                }
            }

            if (chain.Any())
                output.Add(new Node(chain));

            return output;
        }

        internal static List<Node> FlattenNodes(List<Node> chained)
        {
            var output = new List<Node>();

            foreach (var entry in chained)
            {
                if (entry.IsChain)
                {
                    foreach (var item in entry.Nodes)
                    {
                        output.Add(item);
                    }
                }
                else
                {
                    output.Add(entry);
                }
            }

            return output;
        }
    }
}
