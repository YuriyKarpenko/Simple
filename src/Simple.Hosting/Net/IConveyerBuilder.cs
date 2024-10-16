namespace Simple.Hosting.Net;

public interface IConveyerBuilder<TRequestDelegate>
{
    IServiceProvider Services { get; }

    //IFeatureCollection ServerFeatures { get; }

    IConveyerBuilder<TRequestDelegate> Use(Func<TRequestDelegate, TRequestDelegate> middleware);

    TRequestDelegate Build();
}

public class ConveyerBuilderBase<TRequestDelegate> : IConveyerBuilder<TRequestDelegate>
{
    private readonly List<Func<TRequestDelegate, TRequestDelegate>> _components = new();
    private readonly TRequestDelegate _lastDele;

    public ConveyerBuilderBase(IServiceProvider sp, TRequestDelegate lastDelegate)
    {
        Services = sp;
        _lastDele = lastDelegate;
    }

    /// <summary> Gets the <see cref="IServiceProvider"/> for application services. </summary>
    public IServiceProvider Services { get; set; }


    public TRequestDelegate Build()
    {
        var app = _lastDele;

        var c = _components.Count;
        while (c-- > 0)
        {
            app = _components[c](app);
        }

        return app;
    }

    public IConveyerBuilder<TRequestDelegate> Use(Func<TRequestDelegate, TRequestDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }
}