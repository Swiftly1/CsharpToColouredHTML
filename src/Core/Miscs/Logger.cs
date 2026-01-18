namespace CsharpToColouredHTML.Core.Miscs;

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

    public static void PrintCurrentText(string s, int currentIndex, int tabsDepth = 0)
    {
        if (!LogsEnabled)
            return;

        var tabs = new string('\t', tabsDepth);
        Console.Write($"{tabs}[INFO] Current Text: '");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(s);
        Console.ResetColor();
        Console.WriteLine($"' at {currentIndex}");
    }
}
