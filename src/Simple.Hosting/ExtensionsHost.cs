﻿using System.Threading;
using System.Threading.Tasks;

using Simple.DI;

#nullable disable
namespace Simple.Hosting;
public static class ExtensionsHost
{
    /// Starts the host synchronously.
    /// </summary>
    /// <param name="host"></param>
    public static void Start(this IHost host)
    {
        host.StartAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Attempts to gracefully stop the host with the given timeout.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="timeout">The timeout for stopping gracefully. Once expired the
    /// server may terminate any remaining active connections.</param>
    /// <returns></returns>
    public static Task StopAsync(this IHost host, TimeSpan timeout)
    {
        return host.StopAsync(new CancellationTokenSource(timeout).Token);
    }

    /// <summary>
    /// Block the calling thread until shutdown is triggered via Ctrl+C or SIGTERM.
    /// </summary>
    /// <param name="host">The running <see cref="IHost"/>.</param>
    public static void WaitForShutdown(this IHost host)
    {
        host.WaitForShutdownAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs an application and block the calling thread until host shutdown.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to run.</param>
    public static void Run(this IHost host)
    {
        host.RunAsync().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Runs an application and returns a Task that only completes when the token is triggered or shutdown is triggered.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to run.</param>
    /// <param name="token">The token to trigger shutdown.</param>
    public static async Task RunAsync(this IHost host, CancellationToken token = default)
    {
        using (host)
        {
            await host.StartAsync(token).ConfigureAwait(false);

            await host.WaitForShutdownAsync(token).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a Task that completes when shutdown is triggered via the given token.
    /// </summary>
    /// <param name="host">The running <see cref="IHost"/>.</param>
    /// <param name="token">The token to trigger shutdown.</param>
    public static async Task WaitForShutdownAsync(this IHost host, CancellationToken token = default)
    {
        var applicationLifetime = host.Services.GetService<IApplicationLifetime>();

        //  остановка IApplicationLifetime по инициативе token
        token.Register(state =>
        {
            ((IApplicationLifetime)state).StopApplication();
        },
        applicationLifetime);

        var waitForStop = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        //  остановка waitForStop по инициативе IApplicationLifetime.ApplicationStopping
        applicationLifetime.ApplicationStopping.Register(obj =>
        {
            var tcs = (TaskCompletionSource<object>)obj;
            tcs.TrySetResult(null);
        }, waitForStop);

        //  запуск ожидания отмены IApplicationLifetime.ApplicationStopping (ну или token)
        await waitForStop.Task;

        // Host will use its default ShutdownTimeout if none is specified.
        await host.StopAsync();
    }
}