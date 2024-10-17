using System.Threading.Tasks;

using Simple.DI;

namespace Simple.Hosting.Net;

public interface ITcpServerTarget
{
    Task<ReadOnlyMemory<byte>?> AcceptRequestAsync(ReadOnlyMemory<byte> request);
}

public class TcpServerTarget : ITcpServerTarget
{
    public static TcpServerTarget Create(IServiceProvider sp)
    {
        var factory = sp.GetService<ITcpContextFactory>() ?? TcpContextFactory.Instance;
        var tcpNext = sp.GetServiceRequired<RequestDelegate<TcpContext>>();
        return new TcpServerTarget(sp, factory, tcpNext);
    }


    private readonly IServiceProvider _sp;
    private readonly ITcpContextFactory _factory;
    private readonly RequestDelegate<TcpContext> _tcpNext;
    public TcpServerTarget(IServiceProvider sp, ITcpContextFactory factory, RequestDelegate<TcpContext> tcpNext)
    {
        _sp = sp;
        _factory = factory;
        _tcpNext = tcpNext;
    }

    public async Task<ReadOnlyMemory<byte>?> AcceptRequestAsync(ReadOnlyMemory<byte> requestData)
    {
        var context = _factory.CreateTcpContext(_sp, requestData);
        await _tcpNext(context);
        return context.Response.Body;
    }
}