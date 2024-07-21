using System.Threading;
using System.Threading.Tasks;

namespace Simple.Hosting;
public interface IHostedService
{
    /// <summary> Triggered when the application host is ready to start the service. </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous Start operation.</returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary> Triggered when the application host is performing a graceful shutdown. </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous Stop operation.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}