namespace CsharpToColouredHTML.Core.Miscs;

public class Hints
{
    public List<string> BuiltInTypes { get; } =
    [
        "bool",
        "byte",
        "sbyte",
        "char",
        "decimal",
        "double",
        "float",
        "int",
        "uint",
        "nint",
        "nuint",
        "long",
        "ulong",
        "short",
        "ushort",
        "object",
        "string",
        "dynamic",
    ];

    public List<string> ReallyPopularEnums { get; } =
    [
        "PictureBoxSizeMode",
        "ConsoleColor"
    ];

    public List<string> ReallyPopularClasses { get; } =
    [
        "List",
        "Dictionary",
        "Console",
        "Task",
        "Func",
        "Action",
        "Predicate",
        "EventArgs",
        "File",
        "EqualityComparer",
        "Path",
        "GC",
        "Math",
        "Random",
        "String",
        "JsonConvert",
        "Parallel",
        "AppDomain",
        "Screen"
    ];

    public List<string> ReallyPopularClassSubstrings { get; } =
    [
        "Controller",
        "DTO",
        "User",
        "Manager",
        "Handler",
        "Node",
        "Exception",
        "EventHandler"
    ];

    public List<string> ReallyPopularStructs { get; } =
    [
        "CancellationToken",
        "DateTime",
        "TimeOnly",
        "DateOnly",
        "Char",
        "Vector3",
        "ConsoleKeyInfo",
        "IntPtr",
        "Color",
        "Guid"
    ];

    public List<string> ReallyPopularStructsSubstrings { get; } =
    [
        "Span",
    ];
}