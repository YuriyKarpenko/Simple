using System.Threading.Tasks;

using Simple.DI;
namespace Simple.Hosting.Net;
public static class ExtensionsHostBuilder
{
    /// <summary>
    /// Alow using Middleware and register routes for make TRequestDelegate
    /// </summary>
    /// <typeparam name="TRequestDelegate"> Delegate (as RequestDelegate<TcpContext>, RequestDelegate<TCustomContext>). </typeparam>
    /// <param name="hostBuilder"> builder </param>
    /// <param name="lastDelegate"> A finally delegate (usually return NotFound). </param>
    /// <param name="conveyerConfig"> User code for adust TRequestDelegate. </param>
    /// <returns> builder </returns>
    public static IHostBuilder UseConveyer<TRequestDelegate>(this IHostBuilder hostBuilder, TRequestDelegate lastDelegate, Action<IConveyerBuilder<TRequestDelegate>> conveyerConfig)
        => hostBuilder.ConfigureServices(ctx => ctx.ProviderSetup.AddSingleton<TRequestDelegate>(sp =>
        {
            var convBuilder = new ConveyerBuilderBase<TRequestDelegate>(sp, lastDelegate);
            conveyerConfig.Invoke(convBuilder);
            return convBuilder.Build();
        }));

    /// <summary>
    /// Allow using TcpServer with TcpContext
    /// </summary>
    /// <param name="hostBuilder"> builder </param>
    /// <param name="optopns"> Parameters for using TcpServer </param>
    /// <param name="mwFaactory"> The Middleware-factory for extract RequestDelegate<TcpContext> delegate for <see cref="ITcpServerTarget"/>. </param>
    /// <returns> builder </returns>
    public static IHostBuilder UseTcpServer(this IHostBuilder hostBuilder, TcpServerOptopns optopns, Func<IServiceProvider, ITcpMiddleware> mwFaactory)
        => hostBuilder
            .ConfigureServices(c => c.ProviderSetup
                .TryAddSingleton<RequestDelegate<TcpContext>>(sp => c => mwFaactory(sp).InvokeAsync(c, _ => Task.CompletedTask))
                .TryAddSingleton<ITcpServerTarget>(TcpServerTarget.Create))
            .UseHostedService(sp => new TcpServer(sp, optopns));

    /// <summary>
    /// Allow using TcpServer with TcpContext
    /// </summary>
    /// <param name="hostBuilder"> builder </param>
    /// <param name="port"> Port for listen tcp-messages. </param>
    /// <param name="mwFaactory"> The Middleware-factory for extract RequestDelegate<TcpContext> delegate for <see cref="ITcpServerTarget"/>. </param>
    /// <returns> builder </returns>
    public static IHostBuilder UseTcpServer(this IHostBuilder hostBuilder, ushort port, Func<IServiceProvider, ITcpMiddleware> mwFaactory)
        => hostBuilder.UseTcpServer(new TcpServerOptopns { Port = port }, mwFaactory);

    /// <summary>
    /// Allow using TcpServer with TcpContext
    /// must be registered: RequestDelegate<TcpContext>
    /// </summary>
    /// <param name="hostBuilder"> builder </param>
    /// <param name="port"> Port for listen tcp-messages. </param>
    /// <returns> builder </returns>
    public static IHostBuilder UseTcpServer(this IHostBuilder hostBuilder, ushort port)
        => hostBuilder
            .ConfigureServices(c => c.ProviderSetup.TryAddSingleton<ITcpServerTarget>(TcpServerTarget.Create))
            .UseHostedService(sp => new TcpServer(sp, new TcpServerOptopns { Port = port }));
}