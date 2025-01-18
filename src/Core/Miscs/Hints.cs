namespace CsharpToColouredHTML.Core.Miscs;

public class Hints
{
    public List<string> BuiltInTypes { get; } = new List<string>
    {
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
    };

    public List<string> ReallyPopularEnums { get; } = new List<string>
    {
        "PictureBoxSizeMode"
    };

    public List<string> ReallyPopularClasses { get; } = new List<string>
    {
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
        "Parallel"
    };

    public List<string> ReallyPopularClassSubstrings { get; } = new List<string>
    {
        "Controller",
        "DTO",
        "User",
        "Manager",
        "Handler",
        "Node",
        "Exception",
        "EventHandler"
    };

    public List<string> ReallyPopularStructs { get; } = new List<string>
    {
        "CancellationToken",
        "IEnumerable",
        "DateTime",
        "TimeOnly",
        "DateOnly",
        "Char",
        "Vector3",
        "ConsoleKeyInfo",
        "IntPtr",
        "Color",
        "Guid"
    };

    public List<string> ReallyPopularStructsSubstrings { get; } = new List<string>
    {
        "Span",
    };
}