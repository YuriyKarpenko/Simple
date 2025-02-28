using System.Threading;

namespace Simple.Hosting;

/// <summary>
/// Allows consumers to perform cleanup during a graceful shutdown.
/// </summary>
public interface IApplicationLifetime
{
    /// <summary>
    /// Triggered when the application host has fully started and is about to wait
    /// for a graceful shutdown.
    /// </summary>
    CancellationToken ApplicationStarted { get; }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// Requests may still be in flight. Shutdown will block until this event completes.
    /// </summary>
    CancellationToken ApplicationStopping { get; }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// All requests should be complete at this point. Shutdown will block
    /// until this event completes.
    /// </summary>
    CancellationToken ApplicationStopped { get; }

    /// <summary>Requests termination of the current application.</summary>
    void StopApplication();
}

#nullable disable
public class ApplicationLifetime : IApplicationLifetime
{
    private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
    private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
    private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
    private readonly ILogger _logger;

    public ApplicationLifetime(ILogger logger)
    {
        _logger = logger;
    }

    #region IApplicationLifetime

    /// <inheritdoc/>
    public CancellationToken ApplicationStarted => _startedSource.Token;

    /// <inheritdoc/>
    public CancellationToken ApplicationStopping => _stoppingSource.Token;

    /// <inheritdoc/>
    public CancellationToken ApplicationStopped => _stoppedSource.Token;

    /// <inheritdoc/>
    public void StopApplication()
    {
        // Lock on CTS to synchronize multiple calls to StopApplication. This guarantees that the first call 
        // to StopApplication and its callbacks run to completion before subsequent calls to StopApplication, 
        // which will no-op since the first call already requested cancellation, get a chance to execute.
        lock (_stoppingSource)
        {
            try
            {
                ExecuteHandlers(_stoppingSource);
            }
            catch (Exception ex)
            {
                _logger.ErrorMethod(ex, "An error occurred stopping the application".ToString);
            }
        }
    }

    #endregion

    /// <summary>
    /// Signals the ApplicationStarted event and blocks until it completes.
    /// </summary>
    public void NotifyStarted()
    {
        try
        {
            ExecuteHandlers(_startedSource);
        }
        catch (Exception ex)
        {
            _logger.ErrorMethod(ex, "An error occurred started the application".ToString);
        }
    }

    /// <summary>
    /// Signals the ApplicationStopped event and blocks until it completes.
    /// </summary>
    public void NotifyStopped()
    {
        try
        {
            ExecuteHandlers(_stoppedSource);
        }
        catch (Exception ex)
        {
            _logger.ErrorMethod(ex, "An error occurred stopped the application".ToString);
        }
    }

    private void ExecuteHandlers(CancellationTokenSource cancel)
    {
        // Noop if this is already cancelled
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        // Run the cancellation token callbacks
        cancel.Cancel(throwOnFirstException: false);
    }
}