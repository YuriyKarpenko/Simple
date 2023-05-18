using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using Simple.DI;
using Simple.Logging.Configuration;
using Simple.Logging.Logging;
using Simple.Logging.Messages;
using Simple.Logging.Observers;

namespace Simple.Logging
{
    public static class OptionExtensions
    {
        public static ILogger Log(this IEnableLogger logger)
            => LogManager.LoggerFactory.GetLogger(logger.GetType());

        public static ILogger GetLogger(this IServiceProvider _, Type src)
            => LogManager.LoggerFactory.GetLogger(src);
        public static ILogger GetLogger<T>(this IServiceProvider _)
            => LogManager.LoggerFactory.GetLogger<T>();


        /// <summary> Registering logger interfaces </summary>
        public static IProviderSetup AddLogging(this IProviderSetup services)
        {
            var sp = services.BuildServiceProvider();

            //  try to update options
            services.TryRegister(typeof(ILogOptions), () => LogManager.Options);
            LogManager.Options = sp.GetService<ILogOptions>()!;

            //  try to update factory
            services.TryRegister(typeof(ILoggerFactory), () => LogManager.LoggerFactory);
            LogManager.LoggerFactory = sp.GetService<ILoggerFactory>()!;

            //  try to update message factory
            services.TryRegister(typeof(ILogMessageFactory), () => LogManager.MessageFactory);
            LogManager.MessageFactory = sp.GetService<ILogMessageFactory>()!;

            return services;
        }

        public static IProviderSetup AddLogging(this IProviderSetup services, Action<ILogOptions>? configure)
        {
            AddLogging(services);

            configure?.Invoke(LogManager.Options);

            //LogManager.FilterIn = LogManager.Options.FilterOptions.FilterIn;
            LogManager.FilterOut = LogManager.Options.FilterOptions.FilterOut;

            return services;
        }

        /// <summary> Parse logging configuration from JSON </summary>
        /// <param name="options"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ILogOptions AddConfiguration(this ILogOptions options, JToken? config)
        {
            if (options != null && config != null)
            {
                foreach (JProperty jp in config.Children())
                {
                    if (jp.Name.Equals("LogLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load global category defaults
                        options.FilterOptions.Default = LoadRules(jp.Value);
                        continue;
                    }

                    if (jp.Value is JObject jo)
                    {
                        options.ObserversOptions[jp.Name] = jo.ToString();

                        if (jo.GetValue("LogLevel", StringComparison.OrdinalIgnoreCase) is JToken jp2)
                        {
                            // Load logger specific rules
                            options.FilterOptions.Rules[jp.Name] = LoadRules(jp2);
                        }
                    }
                }
            }

            return options;

            LoggerFilterItem LoadRules(JToken configurationSection)
            {
                var d = configurationSection
                    .ToObject<Dictionary<string, string?>>()
                    .ToDictionary(
                        i => i.Key.Equals("Default", StringComparison.OrdinalIgnoreCase) ? string.Empty : i.Key,
                        i => ParseLogLevel(i.Value));
                var rules = new Dictionary<string, LogLevel>(d, StringComparer.OrdinalIgnoreCase);
                return new LoggerFilterItem(rules);
            }
        }

        public static ILogOptions AddConsole(this ILogOptions o)
        {
            LoggingBus.Instance.Subscribe(new ObserverConsole());
            return o;
        }

        public static ILogOptions AddDebug(this ILogOptions o)
        {
            LoggingBus.Instance.Subscribe(new ObserverDebug());
            return o;
        }


        public static LogLevel ParseLogLevel(string? source)
        {
            return source?.ToLowerInvariant() switch
            {
                string s when s.StartsWith("trace") => LogLevel.Trace,
                string s when s.StartsWith("debug") => LogLevel.Debug,
                string s when s.StartsWith("info") => LogLevel.Info,
                string s when s.StartsWith("warn") => LogLevel.Warning,
                string s when s.StartsWith("error") => LogLevel.Error,
                string s when s.StartsWith("critic") => LogLevel.Critical,
                _ => LogLevel.None
            };
        }
    }
}
