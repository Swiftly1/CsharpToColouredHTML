namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal static class Logger
{
#if DEBUG
    public const bool LogsEnabled = true;
#else
    public const bool LogsEnabled = false;
#endif

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
}
