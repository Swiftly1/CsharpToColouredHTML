using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests;
public class FilesTestData : IEnumerable<object[]>
{
    const int TESTS_COUNT = 71;
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var item in Enumerable.Range(1, TESTS_COUNT))
        {
            yield return new object[] { $"{item:0000}.txt" };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
