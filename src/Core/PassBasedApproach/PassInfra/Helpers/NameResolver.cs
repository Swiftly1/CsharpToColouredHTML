using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using static System.Net.Mime.MediaTypeNames;

namespace CsharpToColouredHTML.Core.PassBasedApproach.PassInfra.Helpers;

internal class NameResolver
{
    private SharedPassContext Context;

    public NameResolver(SharedPassContext sharedPassContext)
    {
        this.Context = sharedPassContext;
    }

    public bool IsPopularEnum(string text)
    {
        return Context.Hints.ReallyPopularEnums.Any(x => string.Equals(x, text));
    }

    public bool IsPopularClass(string text)
    {
        return Context.Hints.ReallyPopularClasses.Any(x => string.Equals(x, text))
            ||
            Context.Hints.ReallyPopularClassSubstrings.Any(x => text.Contains(x));
    }

    public bool IsPopularStruct(string text)
    {
        return Context.Hints.ReallyPopularStructs.Any(x => string.Equals(x, text))
            ||
            Context.Hints.ReallyPopularStructsSubstrings.Any(x => text.Contains(x));
    }

    public static bool NameLikeInterface(string text)
    {
        return text.StartsWith("I") && text.Length > 1 && char.IsUpper(text[1]);
    }

    public static bool IsValidClassOrStructName(string text, bool ignoreCase = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.Skip(1).All(x => char.IsLetter(x) || char.IsNumber(x) || x == '_');
    }

    public static readonly string[] CommonKeywordsBeforeTypeName =
    [
        "public", "private", "internal", "sealed", "protected", "readonly", "static", "override", "event", "required",
        "virtual", "unsafe", "partial", "delegate", "async"
    ];

    public static readonly List<string> AccessibilityModifiers = new List<string>
    {
        "public", "private", "protected", "internal", "protected internal", "private protected"
    };

    public static readonly List<string> Operators = new List<string>
    {
        "+", "-", "/", "*", "=", "==", "+=", "-=", "*=", "/=", "!=", "&",
        "^", "|", "&&", "||", "??", "%=", "|=", "^=", "<<=", ">>=", "??=",
        ">>>", ">>>=", "<", ">", "is", "as", ">="
    };

    public static bool SoundsLikeEventOrHandler(string name)
    {
        return name.StartsWith("On") || name.EndsWith("Event") || name.EndsWith("Handler") || name.EndsWith("Changed");
    }

    public string MapColourToClassificationType(string colour, string defaultClassification)
    {
        return colour switch
        {
            NodeColors.Numeric => ClassificationTypeNames.NumericLiteral,
            NodeColors.Method => ClassificationTypeNames.MethodName,
            NodeColors.Class => ClassificationTypeNames.ClassName,
            NodeColors.Keyword => ClassificationTypeNames.Keyword,
            NodeColors.String => ClassificationTypeNames.StringLiteral,
            NodeColors.Control => ClassificationTypeNames.ControlKeyword,
            NodeColors.Interface => ClassificationTypeNames.InterfaceName,
            NodeColors.Comment => ClassificationTypeNames.Comment,
            NodeColors.Preprocessor => ClassificationTypeNames.PreprocessorKeyword,
            NodeColors.PreprocessorText => ClassificationTypeNames.PreprocessorText,
            NodeColors.Struct => ClassificationTypeNames.StructName,
            NodeColors.Namespace => ClassificationTypeNames.NamespaceName,
            NodeColors.EnumMemberName => ClassificationTypeNames.EnumMemberName,
            NodeColors.EnumName => ClassificationTypeNames.EnumName,
            NodeColors.Identifier => ClassificationTypeNames.Identifier,
            NodeColors.Operator => ClassificationTypeNames.Operator,
            NodeColors.PropertyName => ClassificationTypeNames.PropertyName,
            NodeColors.FieldName => ClassificationTypeNames.FieldName,
            NodeColors.LabelName => ClassificationTypeNames.LabelName,
            NodeColors.OperatorOverloaded => ClassificationTypeNames.OperatorOverloaded,
            NodeColors.ConstantName => ClassificationTypeNames.ConstantName,
            NodeColors.ParameterName => ClassificationTypeNames.ParameterName,
            NodeColors.LocalName => ClassificationTypeNames.LocalName,
            NodeColors.ExtensionMethodName => ClassificationTypeNames.ExtensionMethodName,
            NodeColors.TypeParameterName => ClassificationTypeNames.TypeParameterName,
            NodeColors.RecordStructName => ClassificationTypeNames.RecordStructName,
            NodeColors.NumericLiteral => ClassificationTypeNames.NumericLiteral,
            NodeColors.Delegate => ClassificationTypeNames.DelegateName,
            NodeColors.EventName => ClassificationTypeNames.EventName,
            NodeColors.ExcludedCode => ClassificationTypeNames.ExcludedCode,
            _ => defaultClassification
        };
    }

    public void MarkLastElementOfChainAsClassOrStruct(Node type)
    {
        if (type.IsChain)
        {
            var last = type.Nodes.Last();
            Context.MarkNodeAs(last, ResolveClassOrStructName(last));

            foreach (var ns in type.Nodes.SkipLast(1))
            {
                if (ns.ClassificationType == ClassificationTypeNames.Operator)
                {
                    Context.MarkNodeAs(ns, NodeColors.Operator);
                }
                else
                {
                    Context.MarkNodeAs(ns, NodeColors.Namespace);
                }
            }
        }
        else
        {
            Context.MarkNodeAs(type, ResolveClassOrStructName(type));
        }
    }

    public string ResolveUnkownName(Node node)
    {
        var result = CheckIfLooksLikeVariable(node);

        if (result.IsVariable)
            return result.Value;

        var checkResult = IsAlreadyClassifiedExpression(node);

        if (checkResult.Success)
            return checkResult.Value;

        return ResolveClassOrStructName(node);
    }

    public (bool IsVariable, string Value) CheckIfLooksLikeVariable(Node node)
    {
        if (Context.FoundProperties.Contains(node.Text))
            return (IsVariable: true, Value: ResolveVariable(node));

        if (Context.FoundFields.Contains(node.Text))
            return (IsVariable: true, Value: ResolveVariable(node));

        if (node.Text.StartsWith("_"))
            return (IsVariable: true, Value: ResolveVariable(node));

        if (!node.Text.FirstCharIsUpper())
            return (IsVariable: true, Value: ResolveVariable(node));

        return (IsVariable: false, Value: string.Empty);
    }

    public string ResolveClassOrStructName(Node node)
    {
        var checkResult = IsAlreadyClassOrStruct(node);

        if (checkResult.Success)
            return checkResult.Value;

        var text = node.Text;

        if (IsPopularStruct(text))
            return NodeColors.Struct;

        if (IsPopularClass(text))
            return NodeColors.Class;

        if (IsPopularEnum(text) || text.EndsWith("Enum"))
            return NodeColors.EnumName;

        if (Context.FoundClasses.Contains(text))
            return NodeColors.Class;

        if (Context.FoundStructs.Contains(text))
            return NodeColors.Struct;

        if (NameLikeInterface(text))
            return NodeColors.Interface;

        if (Context.Hints.BuiltInTypes.Contains(text))
            return NodeColors.Keyword;

        return NodeColors.Class;
    }

    private static (bool Success, string Value) IsAlreadyClassOrStruct(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.StructName)
            return (Success: true, Value: NodeColors.Struct);

        if (node.ClassificationType == ClassificationTypeNames.ClassName)
            return (Success: true, Value: NodeColors.Class);

        if (node.ClassificationType == ClassificationTypeNames.InterfaceName)
            return (Success: true, Value: NodeColors.Interface);

        if (node.ClassificationType == ClassificationTypeNames.RecordStructName)
            return (Success: true, Value: NodeColors.RecordStructName);

        if (node.ClassificationType == ClassificationTypeNames.RecordClassName)
            return (Success: true, Value: NodeColors.Class);

        return (Success: false, Value: string.Empty);
    }

    public string ResolveVariable(Node node, bool hint_IsClass = false)
    {
        var checkResult = IsAlreadyClassifiedExpression(node);

        if (checkResult.Success)
            return checkResult.Value;

        var text = node.Text;

        if (Context.FoundLocalNames.Contains(text))
            return NodeColors.LocalName;

        if (Context.FoundProperties.Contains(text))
            return NodeColors.PropertyName;

        if (Context.FoundFields.Contains(text))
            return NodeColors.FieldName;

        if (text.FirstCharIsUpper())
        {
            if (hint_IsClass)
            {
                return ResolveClassOrStructName(node);
            }
            return NodeColors.PropertyName;
        }

        if (text.StartsWith("_"))
            return NodeColors.PropertyName;

        return NodeColors.LocalName;
    }

    private (bool Success, string Value) IsAlreadyClassifiedExpression(Node node)
    {
        if (node.ClassificationType == ClassificationTypeNames.LocalName)
            return (Success: true, Value: NodeColors.LocalName);

        if (node.ClassificationType == ClassificationTypeNames.FieldName)
            return (Success: true, Value: NodeColors.FieldName);

        if (node.ClassificationType == ClassificationTypeNames.PropertyName)
            return (Success: true, Value: NodeColors.PropertyName);

        if (node.ClassificationType == ClassificationTypeNames.ConstantName)
            return (Success: true, Value: NodeColors.ConstantName);

        if (node.ClassificationType == ClassificationTypeNames.NumericLiteral)
            return (Success: true, Value: NodeColors.NumericLiteral);

        if (node.ClassificationType == ClassificationTypeNames.StringLiteral)
            return (Success: true, Value: NodeColors.String);

        return (Success: false, Value: string.Empty);
    }

    public string FieldOrProperty(Node node)
    {
        var result = IsAlreadyClassifiedExpression(node);

        if (result.Success)
            return result.Value;

        return NodeColors.PropertyName;
    }
}