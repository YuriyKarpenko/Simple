namespace Simple.Hosting.Net;

public interface ITcpContextFactory
{
    TcpContext CreateTcpContext(IServiceProvider sp, ReadOnlyMemory<byte> requestData);
}

public class TcpContextFactory : ITcpContextFactory
{
    public static ITcpContextFactory Instance = new TcpContextFactory();


    public TcpContext CreateTcpContext(IServiceProvider sp, ReadOnlyMemory<byte> requestData)
        => new TcpContext(sp, new TcpRequest(requestData), new TcpResponse());
}