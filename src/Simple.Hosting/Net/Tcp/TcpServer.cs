using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Hosting.Net;

/// <summary> 
/// Обмен пакетами byte[], где первые 4 байта содержат длину пакета (НЕ включая эти 4 байта).
/// Необходимо зарегистрировать в DI [RequestDelegate<TcpContext>, ITcpServerTarget]
/// </summary>
public class TcpServer : IHostedService
{
    public static TcpServer Create(IServiceProvider sp)
    {
        var option = sp.GetServiceRequired<TcpServerOptopns>();
        return new TcpServer(sp, option);
    }


    private readonly IServiceProvider _sp;
    private readonly ILogger _logger;
    private readonly TcpServerOptopns _serverOptions;
    private readonly IPEndPoint _endPoint;
    private Socket _socket;

    public TcpServer(IServiceProvider sp, TcpServerOptopns options)
    {
        _ = sp.GetServiceRequired<ITcpServerTarget>();  //  check registration
        _sp = sp;
        _logger = sp.CreateLogger<TcpServer>();
        _serverOptions = Throw.IsArgumentNullException(options, nameof(options));
        _endPoint = new IPEndPoint(IPAddress.Any, options.Port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    #region IServer

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _socket.Bind(_endPoint);
        _socket.Listen(100);

        Console.WriteLine($"{nameof(TcpServer)} is started {_socket.LocalEndPoint}");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var socket = await _socket.AcceptAsync(cancellationToken);
                ThreadPool.QueueUserWorkItem<(CancellationToken cancellationToken, Socket socket, IServiceProvider sp)>(ClientProcrssingAsync, (cancellationToken, socket, _sp), true);
            }
            catch (Exception ex)
            {
                _logger.ErrorMethod(ex, _endPoint.ToString);
            }
        }
        _socket.Shutdown(SocketShutdown.Both);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _socket.Close();
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _socket.Dispose();
    }

    #endregion


    private static async void ClientProcrssingAsync((CancellationToken cancellationToken, Socket socket, IServiceProvider sp) p)
    {
        var _logget = p.sp.CreateLogger<TcpServer>();
        var _buffers = MemoryPool<byte>.Shared;
        var sizeOut = p.socket.SendBufferSize;
        var target = p.sp.GetServiceRequired<ITcpServerTarget>();
        using var ownerRead = _buffers.Rent(p.socket.ReceiveBufferSize * 4);
        var sizeMem = new byte[4].AsMemory();
        try
        {
            while (!p.cancellationToken.IsCancellationRequested)
            {
                var rc = await p.socket.ReceiveAsync(sizeMem, SocketFlags.None, p.cancellationToken);

                if (rc == 0)
                {
                    break;
                }
                var contentSize = BitConverter.ToInt32(sizeMem.Span);

                Memory<byte> request;

                #region read

                if (contentSize < ownerRead.Memory.Length)
                {
                    rc = await p.socket.ReceiveAsync(ownerRead.Memory, SocketFlags.None, p.cancellationToken);
                    if (rc != contentSize)
                    {
                        Console.WriteLine($"{nameof(TcpServer)}.{nameof(ClientProcrssingAsync)}() => expected {contentSize} but received {rc}");
                    }
                    request = ownerRead.Memory[..contentSize];
                }
                else
                {
                    request = _buffers.Rent(contentSize).Memory;
                    //  save alreaddy readed
                    var start = 0;
                    contentSize -= 4;
                    do
                    {
                        rc = await p.socket.ReceiveAsync(request[start..], SocketFlags.None, p.cancellationToken);
                        start += rc;
                        contentSize -= rc;
                    } while (rc > 0 && contentSize > 0);
                }

                #endregion

                var response = await target.AcceptRequestAsync(request);

                if (!p.cancellationToken.IsCancellationRequested && response.HasValue)
                {
                    #region write

                    var size = response.Value.Length;
                    BitConverter.TryWriteBytes(sizeMem.Span, size);
                    var l = await p.socket.SendAsync(sizeMem, SocketFlags.None);
                    if (l != 4)
                    {
                        Throw.Exception(new DataMisalignedException("expected 4 but send " + l));
                    }
                    var resp = response.Value;
                    while (resp.Length > 0)
                    {
                        var toSend = resp.Length > sizeOut ? sizeOut : resp.Length;
                        l = await p.socket.SendAsync(resp[..toSend], SocketFlags.None);
                        resp = resp[l..];
                    }

                    #endregion
                }
            }
        }
        catch (Exception ex)
        {
            _logget.ErrorMethod(ex, p.socket.RemoteEndPoint!.ToString);
        }

        p.socket.Shutdown(SocketShutdown.Both);
        p.socket.Close();
        p.socket.Dispose();
    }
}

public class TcpServerOptopns
{
    public TcpServerOptopns Cofigure(Action<TcpServerOptopns>? configure = null)
    {
        configure?.Invoke(this);
        return this;
    }

    public ushort Port { get; set; }
}