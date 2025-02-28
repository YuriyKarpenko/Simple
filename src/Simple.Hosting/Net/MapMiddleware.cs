using System.Threading.Tasks;

namespace Simple.Hosting.Net;

internal class MapMiddleware<TContext>
{
    private readonly RequestDelegate<TContext> _next;
    private readonly MapOptions<TContext> _options;
    public MapMiddleware(RequestDelegate<TContext> next, MapOptions<TContext> options)
    {
        _next = Throw.IsArgumentNullException(next, nameof(next));
        _options = Throw.IsArgumentNullException(options, nameof(options));
    }


    public async Task InvokeAsync(TContext context)
    {
        Throw.IsArgumentNullException(context, nameof(context));
        if (_options.Match(context))
        {
            await _options.Branch(context);
        }
        else
        {
            await _next(context);
        }
    }
}

internal readonly struct MapOptions<TContext>(Func<TContext, bool> match, RequestDelegate<TContext> branch)
{
    public readonly Func<TContext, bool> Match = match;
    public readonly RequestDelegate<TContext> Branch = branch;
}

public static class ExtensionsMapMiddleware
{
    public static IConveyerBuilder<RequestDelegate<TContext>> Map<TContext>(this IConveyerBuilder<RequestDelegate<TContext>> app, Func<TContext, bool> match, RequestDelegate<TContext> branch)
    {
        Throw.IsArgumentNullException(app, nameof(app));
        Throw.IsArgumentNullException(branch, nameof(branch));

        var options = new MapOptions<TContext>(match, branch);
        return app.Use((next) => new MapMiddleware<TContext>(next, options).InvokeAsync);
    }
    public static IConveyerBuilder<RequestDelegate<TContext>> Map<TContext>(this IConveyerBuilder<RequestDelegate<TContext>> app, Func<TContext, bool> match, IController<TContext> controller)
        => Map(app, match, controller.InvokeAsync);
    public static IConveyerBuilder<RequestDelegate<TContext>> Map<TContext>(this IConveyerBuilder<RequestDelegate<TContext>> app, Func<TContext, bool> match, Func<IServiceProvider, IController<TContext>> controllerFactory)
        => Map(app, match, controllerFactory(app.Services));

    public static IConveyerBuilder<RequestDelegate<TContext>> UseMiddleware<TContext>(this IConveyerBuilder<RequestDelegate<TContext>> app, IMiddleware<TContext> mw)
        where TContext : IContext
        => app.Use(next => context => Throw.IsArgumentNullException(mw, nameof(mw)).InvokeAsync(context, next));

    public static IConveyerBuilder<RequestDelegate<TContext>> UseMiddleware<TContext>(this IConveyerBuilder<RequestDelegate<TContext>> app, Func<IServiceProvider, IMiddleware<TContext>> mwFactory)
        where TContext : IContext
        => app.UseMiddleware(mwFactory(app.Services));
}