using System;
using System.IO;
using System.Threading.Tasks;

namespace Test.Helpers;

public class ThrowTests
{
    [Fact]
    public void IsArgumentNullException_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Throw.IsArgumentNullException((object?)null));
    }

    [Fact]
    public void IsArgumentNullException_NonNull_ReturnsValue()
    {
        var value = new object();

        var result = Throw.IsArgumentNullException(value);

        Assert.Same(value, result);
    }

    [Fact]
    public void IsArgumentNullException_WithInvalidPredicate_Throws()
    {
        Assert.Throws<ArgumentException>(() => Throw.IsArgumentNullException(5, i => i > 10, "value"));
    }

    [Fact]
    public void IsArgumentNullException_WithValidPredicate_ReturnsValue()
    {
        var result = Throw.IsArgumentNullException(5, i => i > 1, "value");

        Assert.Equal(5, result);
    }

    [Fact]
    public void ArgumentException_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => Throw.ArgumentException("param"));

        Assert.Equal("param", ex.ParamName);
        Assert.Contains("param", ex.Message);
    }

    [Fact]
    public void Exception_ThrowsPassedInstance()
    {
        var source = new InvalidOperationException("boom");

        var ex = Assert.Throws<InvalidOperationException>(() => Throw.Exception(source));

        Assert.Same(source, ex);
    }

    [Fact]
    public void Exception_Generic_ThrowsPassedInstance()
    {
        var source = new IOException("boom");

        var ex = Assert.Throws<IOException>(() => Throw.Exception<object>(source));

        Assert.Same(source, ex);
    }

    [Fact]
    public async Task ExceptionAsync_ReturnsFaultedTask()
    {
        var source = new InvalidOperationException("boom");

        var task = Throw.ExceptionAsync<object>(source);

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }
}