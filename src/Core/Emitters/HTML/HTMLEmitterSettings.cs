namespace CsharpToColouredHTML.Core.Emitters.HTML;

public class HTMLEmitterSettings
{
    public string? UserProvidedCSS = null;

    public bool AddLineNumber = true;

    public bool Optimize = true;

    public bool UseIframe = true;

    public Func<int, bool>? LineHighlightingPredicate = null;

    public HTMLEmitterSettings()
    {
    }

    public HTMLEmitterSettings UseDefaultCSS()
    {
        UserProvidedCSS = null;
        return this;
    }

    public HTMLEmitterSettings UseCustomCSS(string css)
    {
        if (css == null)
            throw new ArgumentException(nameof(css));

        UserProvidedCSS = css;
        return this;
    }

    public HTMLEmitterSettings DisableLineNumbers()
    {
        AddLineNumber = false;
        return this;
    }

    public HTMLEmitterSettings EnableLineNumbers()
    {
        AddLineNumber = true;
        return this;
    }

    public HTMLEmitterSettings DisableOptimizations()
    {
        Optimize = false;
        return this;
    }

    public HTMLEmitterSettings EnableOptimizations()
    {
        Optimize = true;
        return this;
    }

    public HTMLEmitterSettings EnableIframe()
    {
        UseIframe = true;
        return this;
    }

    public HTMLEmitterSettings DisableIframe()
    {
        UseIframe = false;
        return this;
    }

    public HTMLEmitterSettings HighlightThoseLines(Func<int, bool> func)
    {
        if (func == null)
            throw new ArgumentException(nameof(func));

        LineHighlightingPredicate = func;
        return this;
    }

    public HTMLEmitterSettings RemoveHighlighting()
    {
        LineHighlightingPredicate = null;
        return this;
    }
}
