namespace CsharpToColouredHTML.Core.Miscs;
#pragma warning disable CS0162

internal static class Logger
{
#if DEBUG
    public const bool LogsEnabled = true;
#else
    public const bool LogsEnabled = false;
#endif

    public static void Success(string s, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] {tabs}{s}");
        Console.ResetColor();
    }

    public static void Info(string s, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        Console.WriteLine($"[INFO] {tabs}{s}");
    }

    public static void Warning(string s, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        Console.WriteLine($"[WARN] {tabs}{s}");
    }

    public static void Error(string s, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        Console.WriteLine($"[ERROR] {tabs}{s}");
    }

    public static void PrintFancy(string part1, string part2, string part3, ConsoleColor colour, bool addInfo = true, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        var info = addInfo ? "[INFO] " : string.Empty;
        Console.Write(info + tabs + part1);
        Console.ForegroundColor = colour;
        Console.Write(part2);
        Console.ResetColor();
        Console.WriteLine(part3);
    }
}
