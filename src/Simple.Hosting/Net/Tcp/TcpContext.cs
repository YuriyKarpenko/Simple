namespace Simple.Hosting.Net;

public class TcpContext : ContextBase<TcpRequest, TcpResponse>
{
    public TcpContext(IServiceProvider sp, TcpRequest request, TcpResponse response) : base(sp, request, response)
    {
    }
}

public class TcpRequest(ReadOnlyMemory<byte> body)
{
    public virtual ReadOnlyMemory<byte> Body { get; set; } = body;
}

public class TcpResponse
{
    public virtual ReadOnlyMemory<byte>? Body { get; set; }
}