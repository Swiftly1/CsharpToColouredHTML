﻿using System.Collections;
using System.Collections.Generic;

namespace Tests;
public class FilesTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "0001.txt" };
        yield return new object[] { "0002.txt" };
        yield return new object[] { "0003.txt" };
        yield return new object[] { "0004.txt" };
        yield return new object[] { "0005.txt" };
        yield return new object[] { "0006.txt" };
        yield return new object[] { "0007.txt" };
        yield return new object[] { "0008.txt" };
        yield return new object[] { "0009.txt" };
        yield return new object[] { "0010.txt" };
        yield return new object[] { "0011.txt" };
        yield return new object[] { "0012.txt" };
        yield return new object[] { "0013.txt" };
        yield return new object[] { "0014.txt" };
        yield return new object[] { "0015.txt" };
        yield return new object[] { "0016.txt" };
        yield return new object[] { "0017.txt" };
        yield return new object[] { "0018.txt" };
        yield return new object[] { "0019.txt" };
        yield return new object[] { "0020.txt" };
        yield return new object[] { "0021.txt" };
        yield return new object[] { "0022.txt" };
        yield return new object[] { "0023.txt" };
        yield return new object[] { "0024.txt" };
        yield return new object[] { "0025.txt" };
        yield return new object[] { "0026.txt" };
        yield return new object[] { "0027.txt" };
        yield return new object[] { "0028.txt" };
        yield return new object[] { "0029.txt" };
        yield return new object[] { "0030.txt" };
        yield return new object[] { "0031.txt" };
        yield return new object[] { "0032.txt" };
        yield return new object[] { "0033.txt" };
        yield return new object[] { "0034.txt" };
        yield return new object[] { "0035.txt" };
        yield return new object[] { "0036.txt" };
        yield return new object[] { "0037.txt" };
        yield return new object[] { "0038.txt" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
