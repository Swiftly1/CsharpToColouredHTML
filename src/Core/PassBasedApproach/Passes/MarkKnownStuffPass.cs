using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra;
using CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Enumeration;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.PassBasedApproach.Passes.MarkKnownStuff;

internal class MarkKnownStuffPass : Pass
{
    // The purpose of this pass is to collect as much reliable hints as possible
    // In order to use them in other passes so they can generate better heuristics.

    public override string Name { get => "MarkKnownStuff"; }

    private NodeEnumerationHelper? Walker { get; set; }

    public MarkKnownStuffPass(SharedPassContext ctx) : base(ctx)
    {
    }

    public override PassResult Run(List<Node> input)
    {
        var flatten = NodeChaining.FlattenNodes(input);
        Walker = new NodeEnumerationHelper(flatten);

        do
        {
            if (_SimpleClassificationToColourMapper.TryGetValue(Walker.CC, out var simpleColour))
            {
                Context.MarkNodeAs(Walker.CurrentNode, simpleColour, true);
                continue;
            }

            if (Context.Hints.BuiltInTypes.Contains(Walker.CurrentText))
            {
                Context.MarkNodeAs(Walker.CurrentNode, NodeColors.Keyword, true);
                continue;
            }
        } while (Walker.MoveNext());


        return new PassResult();
    }

    private Dictionary<string, string> _SimpleClassificationToColourMapper { get; } = new()
    {
        { ClassificationTypeNames.PreprocessorKeyword, NodeColors.Preprocessor },
        { ClassificationTypeNames.PreprocessorText, NodeColors.PreprocessorText },
        { ClassificationTypeNames.InterfaceName, NodeColors.Interface },
        { ClassificationTypeNames.NamespaceName, NodeColors.Namespace },
        { ClassificationTypeNames.EnumName, NodeColors.EnumName },
        { ClassificationTypeNames.Operator, NodeColors.Operator },
        { ClassificationTypeNames.ControlKeyword, NodeColors.Control },
        { ClassificationTypeNames.EnumMemberName, NodeColors.EnumMemberName },
        { ClassificationTypeNames.StringLiteral, NodeColors.String },
        { ClassificationTypeNames.VerbatimStringLiteral, NodeColors.String },
        { ClassificationTypeNames.LocalName, NodeColors.LocalName },
        { ClassificationTypeNames.MethodName, NodeColors.Method },
        { ClassificationTypeNames.PropertyName, NodeColors.PropertyName },
        { ClassificationTypeNames.ParameterName, NodeColors.ParameterName },
        { ClassificationTypeNames.FieldName, NodeColors.FieldName },
        { ClassificationTypeNames.NumericLiteral, NodeColors.NumericLiteral },
        { ClassificationTypeNames.LabelName, NodeColors.LabelName },
        { ClassificationTypeNames.OperatorOverloaded, NodeColors.OperatorOverloaded },
        { ClassificationTypeNames.TypeParameterName, NodeColors.TypeParameterName },
        { ClassificationTypeNames.ExtensionMethodName, NodeColors.ExtensionMethodName },
        { ClassificationTypeNames.ConstantName, NodeColors.ConstantName },
        { ClassificationTypeNames.DelegateName, NodeColors.Delegate },
        { ClassificationTypeNames.EventName, NodeColors.EventName },
        { ClassificationTypeNames.ExcludedCode, NodeColors.ExcludedCode },

        { ClassificationTypeNames.RecordClassName, NodeColors.Class },
        { ClassificationTypeNames.ClassName, NodeColors.Class },
        { ClassificationTypeNames.StructName, NodeColors.Struct },
        { ClassificationTypeNames.RecordStructName, NodeColors.RecordStructName },
        { ClassificationTypeNames.Keyword, NodeColors.Keyword },
        { ClassificationTypeNames.Punctuation, NodeColors.Punctuation },

        { ClassificationTypeNames.Comment, NodeColors.Comment },
        { ClassificationTypeNames.RegexComment, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentAttributeName, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentAttributeQuotes, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentAttributeValue, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentCDataSection, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentComment, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentDelimiter, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentEntityReference, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentName, NodeColors.Comment },
        { ClassificationTypeNames.XmlLiteralProcessingInstruction, NodeColors.Comment },
        { ClassificationTypeNames.XmlDocCommentText, NodeColors.Comment },
        { ClassificationTypeNames.XmlLiteralComment, NodeColors.Comment },

        //{ ClassificationTypeNames.Identifier, NodeColors.Identifier },
    };
}