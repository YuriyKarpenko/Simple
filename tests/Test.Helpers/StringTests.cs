using System;
using System.Collections.Generic;

namespace Test.Helpers;

public class StringTests
{
    [Theory]
    [InlineData(typeof(List<>), "List<>")]
    [InlineData(typeof(Dictionary<string, List<int>>), "Dictionary<String, List<Int32>>")]
    void GetClassName(Type type, string expected)
    {
        //  test
        var actual = type.GetClassName();

        //  assert
        Assert.Equal(expected, actual);
    }
}