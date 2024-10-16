namespace Simple.Hosting.Net;

public interface IContext
{
    string SessionId { get; }
    Dictionary<string, object> Items { get; }
    IServiceProvider RequestServices { get; }
}

public interface IContext<TIn, TOut> : IContext
{
    TIn Request { get; }
    TOut Response { get; }
}

public class ContextBase<TIn, TOut> : IContext<TIn, TOut>
{
    public ContextBase(IServiceProvider sp, TIn request, TOut response, string? sessionId = null)
    {
        SessionId = sessionId ?? Guid.NewGuid().ToString();
        Items = new Dictionary<string, object>();
        RequestServices = sp;
        Request = request;
        Response = response;
    }

    public virtual string SessionId { get; }

    public Dictionary<string, object> Items { get; }
    public IServiceProvider RequestServices { get; }

    public TIn Request { get; }
    public TOut Response { get; }
}