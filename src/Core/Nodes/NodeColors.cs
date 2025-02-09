namespace CsharpToColouredHTML.Core.Nodes;

public static class NodeColors
{
    public const string Background = "background";

    public const string Numeric = "numeric";

    public const string Method = "method";

    public const string Class = "class";

    public const string Keyword = "keyword";

    public const string String = "string";

    public const string Control = "control";

    public const string Interface = "interface";

    public const string Comment = "comment";

    public const string Preprocessor = "preprocessor";

    public const string PreprocessorText = "preprocessorText";

    public const string Struct = "struct";

    public const string Namespace = "namespace";

    public const string EnumMemberName = "enumMember";

    public const string Identifier = "identifier";

    public const string Punctuation = "punctuation";

    public const string Operator = "operator";

    public const string PropertyName = "propertyName";

    public const string FieldName = "fieldName";

    public const string LabelName = "labelName";

    public const string OperatorOverloaded = "operator_overloaded";

    public const string ConstantName = "constant";

    public const string ParameterName = "parameter";

    public const string LocalName = "localName";

    public const string ExtensionMethodName = "extension";

    public const string TypeParameterName = "typeParam";

    public const string RecordStructName = "recordStruct";

    public const string NumericLiteral = "numericLiteral";

    public const string EnumName = "enumName";

    public const string Delegate = "delegate";

    public const string EventName = "eventName";

    public const string ExcludedCode = "excludedCode";

    public const string InternalError = "internalError";

    public const string Default = "default";

#if DEBUG
    public const string DefaultColour = NodeColors.InternalError;
#else
    public const string DefaultColour = NodeColors.Default;
#endif
}
