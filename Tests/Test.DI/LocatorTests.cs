using Simple.DI;

namespace Test.DI;

public class LocatorTests
{
    [Fact]
    public void Setup_NullAction_Throws()
    {
        //  assert
        Assert.Throws<ArgumentNullException>(() => Locator.Setup(null!));
    }

    [Fact]
    public void Setup_Action_RegistersOnSharedContainer()
    {
        //  test
        var sp = Locator.Setup(s => s.AddSingleton(() => new Uri("https://simple.di/test")));

        //  assert
        Assert.Same(Locator.Current, sp);
        Assert.IsType<Uri>(sp.GetService(typeof(Uri)));
    }

    [Fact]
    public void Setup_WithoutAction_ReturnsSharedContainer()
    {
        //  test + assert
        Assert.Same(Locator.Current, Locator.Setup());
    }

}
