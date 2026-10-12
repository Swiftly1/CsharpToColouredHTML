using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Tests;
public class FilesTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var disabledTests = new List<string> { };
        var enabledTests = new DirectoryInfo(E2E.InputDir)
                           .GetFiles()
                           .Select(x => x.Name)
                           .Where(x => !disabledTests.Contains(x))
                           .ToList();

        foreach (var item in enabledTests)
            yield return new object[] { item };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
