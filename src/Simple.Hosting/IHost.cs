using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Simple.Configuration;
using Simple.DI;

namespace Simple.Hosting;

public interface IHost : IDisposable
{
    /// <summary> The <see cref="IServiceProvider"/> for the host </summary>
    IServiceProvider Services { get; }

    /// <summary> Starts application </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}

public class Host : IHost
{
    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

                //if (args != null)
                //{
                //    config.AddCommandLine(args);
                //}
            })
            .ConfigureLogging((hostingContext, logging) =>
                logging.AddConfiguration(hostingContext.Configuration.Json.GetValue("Logging"))
                    .AddConsole()
                    .AddDebug()
            );

        return builder;
    }

    protected readonly IConfiguration _config;
    protected readonly IProviderSetup _setup;
    protected readonly ILogger _logger;
    protected readonly ApplicationLifetime _applicationLifetime;
    private IHostedService? _hostedService;


    public Host(HostBuilderContext context)
    {
        Throw.IsArgumentNullException(context, nameof(context));

        _config = Throw.IsArgumentNullException(context.Configuration, nameof(context.Configuration));

        _setup = Throw.IsArgumentNullException(context.ProviderSetup, nameof(context.ProviderSetup));
        Services = _setup.BuildServiceProvider();

        _logger = Services.CreateLogger(GetType());


        _applicationLifetime = new ApplicationLifetime(Services.CreateLogger<ApplicationLifetime>());
        _setup.AddSingleton<IApplicationLifetime>(_applicationLifetime);
    }

    #region IHost

    public virtual IServiceProvider Services { get; }

    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.DebugMethod(() => "Starting");

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
        using (linkedCts)
        {
            var token = linkedCts.Token;

            _hostedService = Services.GetServiceRequired<IHostedService>();
            await _hostedService.StartAsync(token).ConfigureAwait(false);

            _applicationLifetime?.NotifyStarted();
        }

        //var application = BuildApplication();

        //_applicationLifetime = _applicationServices.GetRequiredService<IApplicationLifetime>() as ApplicationLifetime;
        //_hostedServiceExecutor = _applicationServices.GetRequiredService<HostedServiceExecutor>();
        //var diagnosticSource = _applicationServices.GetRequiredService<DiagnosticListener>();
        //var httpContextFactory = Services.GetServiceRequired<IHttpContextFactory>();
        //var hostingApp = new HostingApplication(application, _logger, diagnosticSource, httpContextFactory);
        //await Server.StartAsync(hostingApp, cancellationToken).ConfigureAwait(false);

        // Fire IApplicationLifetime.Started
        // Fire IHostedService.Start
        //await _hostedServiceExecutor.StartAsync(cancellationToken).ConfigureAwait(false);

        _logger.DebugMethod(() => "Started");
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.DebugMethod(() => "Stopping");

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using (linkedCts)
        {
            var token = linkedCts.Token;

            Debug.Assert(_hostedService != null, "Hosted service are resolved when host is started.");

            // Fire IApplicationLifetime.Stopping
            _applicationLifetime?.StopApplication();

            await _hostedService!.StopAsync(token).ConfigureAwait(false);
        }

        // Fire IApplicationLifetime.Stopped
        _applicationLifetime?.NotifyStopped();

        _logger.DebugMethod(() => "Stopped");
    }

    #endregion

    public void Dispose()
    {
        (Services as IDisposable)?.Dispose();
    }
}