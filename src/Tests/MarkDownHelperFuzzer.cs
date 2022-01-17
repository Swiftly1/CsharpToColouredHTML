using Xunit;
using System;
using System.Linq;
using System.Reflection;
using CsharpToColouredHTML.Miscs;
using System.Collections.Generic;

namespace Tests
{
    public class MarkDownHelperFuzzer
    {
        private Random rnd = new Random();

        //[Fact]
        public void Fuzzer()
        {
            var opening = "```csharp";

            for (int i = 0; i < 100_000; i++)
            {
                var length = rnd.Next(0, 400);
                var str = RandomString(length);

                var insertedCount = rnd.Next(0, 15);

                if (str.Length > 0)
                {
                    for (int j = 0; j < insertedCount; j++)
                    {
                        var rndIndex = rnd.Next(0, str.Length);

                        while (opening.Contains(str[rndIndex]))
                            rndIndex = rnd.Next(0, str.Length);

                        str = str.Insert(rndIndex, opening);
                    }
                }

                if (length == 0)
                {
                    var exception = Record.Exception(() =>
                    {
                        var result = ExecutePrivateMethod(opening, str);
                    });

                    Assert.Contains("String or substring is not specified.", exception.ToString());
                }
                else
                {
                    var result = ExecutePrivateMethod(opening, str);
                    Assert.Equal(insertedCount, result.Count);
                }
            }
        }

        [Fact]
        public void Test()
        {
            var opening = "```csharp";

            var str = opening + opening;

            var result = ExecutePrivateMethod(opening, str);

            Assert.Equal(2, result.Count);
        }

        private static List<int>? ExecutePrivateMethod(string opening, string str)
        {
            return MarkdownHelper.AllIndicesOf(str, opening);
        }

        public string RandomString(int length)
        {
            const string chars = "ABC🐉🐒🐉뉴스가 전🧔🎄🎂세계 매체로부터종합한🟫⬛최신 뉴스 헤이뉴스-스토리커뮤니티DE🐒FGHIJKLMNOPQRS@#$#^$%&^*&)(*_)(!@#!@TUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
