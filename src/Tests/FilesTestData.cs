using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests;
public class FilesTestData : IEnumerable<object[]>
{
    const int TESTS_COUNT = 81;
    public IEnumerator<object[]> GetEnumerator()
    {
        var disabledTests = new List<int> { 59 };
        foreach (var item in Enumerable.Range(1, TESTS_COUNT))
        {
            if (disabledTests.Contains(item))
                continue;

            yield return new object[] { $"{item:0000}.txt" };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
