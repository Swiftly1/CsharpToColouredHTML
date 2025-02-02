using System.Diagnostics;

namespace CsharpToColouredHTML.Core.Miscs;

public static class StringHelper
{
    [DebuggerStepThrough]
    public static List<int> AllIndicesOf(string str, string substr)
    {
        if (str is null)
        {
            throw new ArgumentException(nameof(str));
        }

        if (substr is null)
        {
            throw new ArgumentException(nameof(substr));
        }

        if (string.IsNullOrEmpty(substr))
        {
            return new List<int>();
        }

        if (string.IsNullOrEmpty(substr))
        {
            return new List<int>();
        }

        var indexes = new List<int>();
        int index = 0;

        while ((index = str.IndexOf(substr, index)) != -1)
        {
            indexes.Add(index++);
        }

        return indexes;
    }

    [DebuggerStepThrough]
    public static bool EqualsAnyOf(this string s, params string[] strs)
    {
        if (strs == null)
            return false;

        foreach (var val in strs)
        {
            if (s.Equals(val))
                return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public static bool ContainsAnyOf(this string s, params string[] strs)
    {
        if (strs == null)
            return false;

        foreach (var val in strs)
        {
            if (s.Contains(val))
                return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public static bool IndexIsValid<T>(this List<T> list, int index, out T entry) where T : class
    {
        entry = null;

        if (index < 0) return false;
        if (index >= list.Count) return false;

        entry = list[index];
        return true;
    }
}