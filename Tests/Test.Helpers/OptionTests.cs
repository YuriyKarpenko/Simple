using System;

namespace Test.Helpers;

public class OptionTests
{
    private const string argName = "some arg";
    #region Create

    [Theory]
    [InlineData(null, false)]
    [InlineData("1", false)]
    [InlineData("2", true)]
    [InlineData("2some", true)]
    public void CreateStr(string? value, bool expected)
    {
        var actual = Option.String(value).Validate(i => i?.StartsWith("2") == true, argName);
        CreateAssert(actual, value, expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    public void CreateInt(int? value, bool expected)
    {
        var actual = Option.Value(value).NotNull(argName);

        Assert.Equal(expected, actual.HasValue);
        if (expected)
        {
            Assert.Equal(value, actual.Value);
            Assert.Equal(Option.MsgNoError, actual.GetError());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => actual.Value);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    public void CreateObj(object? value, bool expected)
    {
        var actual = Option.Value(value).NotNull(argName);
        CreateAssert(actual, value, expected);
    }

    private static void CreateAssert<T>(IOption<T> actual, T? value, bool expected) where T : notnull
    {
        Assert.Equal(expected, actual.HasValue);
        if (expected)
        {
            Assert.Equal(value, actual.Value);
            Assert.Equal(Option.MsgNoError, actual.GetError());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => actual.Value);
        }
    }

    #endregion

    #region Error

    [Theory]
    [InlineData(null)]
    [InlineData("some")]
    public void ErrorFunc(string? message)
    {
        //  arrange
        var expected = Option.MessageMethodFormat(() => message);

        //  test
        var actual = Option.Error<object>(() => message);

        //  assert
        ErrorAssert(actual, expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("some")]
    public void ErrorMessage(string? message)
    {
        //  arrange
        var expected = Option.MessageMethodFormat(() => message);

        //  test
        var actual = Option.Error<object>(message);

        //  assert
        ErrorAssert(actual, expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("some arg", "some message")]
    public void ErrorException(string? args, string? message)
    {
        //  arrange
        var ex = new Exception(message);
        var expected = $"ErrorException({args}) => {message}";

        //  test
        var actual = Option.Error<object>(ex, () => args);

        //  assert
        Assert.Throws<InvalidOperationException>(() => actual.Value);
        Assert.StartsWith(expected, actual.GetError());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("some")]
    public void ErrorOption(string? message)
    {
        //  arrange
        var o = Option.Error<int>(message);
        var expected = Option.MessageMethodFormat(() => message);

        //  test
        var actual = Option.Error<object>(o);

        //  assert
        ErrorAssert(actual, expected);
    }

    private static void ErrorAssert<T>(IOption<T> actual, string expectedError)
    {
        Assert.Throws<InvalidOperationException>(() => actual.Value);
        Assert.Equal(expectedError, actual.GetError());
    }

    #endregion

    [Theory]
    [InlineData(null, null)]
    [InlineData("some arg", 2)]
    public void Join(string? valueA, int? valueB)
    {
        //  arrange
        var oa = Option.Value(valueA);
        var ob = Option.Value(valueB);
        var expected = (a: valueA, b: valueB);

        //  test
        var actual = oa.Join(ob, (a, b) => Option.Value((a, b)));

        //  assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("some")]
    public void Try(string? error)
    {
        const int value = 2;
        //  arrange
        var hasVaue = string.IsNullOrEmpty(error);

        //  test
        var actual = Option.Try(() => hasVaue ? value : Throw.Exception<int>(new Exception(error)));

        //  assert
        Assert.Equal(hasVaue, actual.HasValue);
        if (hasVaue)
        {
            Assert.Equal(value, actual.Value);
        }
        else
        {
            Assert.StartsWith(Option.MessageMethodFormat(() => "Option.Try()", () => $" => {error}"), actual.GetError());
        }
    }


    //  TODO: make other
    #region Then

    [Theory]
    [InlineData(null, null)]
    [InlineData(2, 4)]
    public void Then(int? value, int? expected)
    {
        const int append = 2;
        //  arrange
        var o = Option.Value(value);

        //  test
        var actual = o.Then(i => Option.Value(i + append)).Value;

        //  assert
        Assert.Equal(expected, actual);
    }


    #endregion
}
