using Newtonsoft.Json.Linq;

using Simple.DI;
using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging
{
    public static class ExtensionsLogOption
    {
        /// <summary> Registering logger interfaces </summary>
        public static IProviderSetup AddLogging(this IProviderSetup services)
        {
            var sp = services.BuildServiceProvider();

            //  try to update options
            services.TryRegister(typeof(ILogOptions), () => LogManager.Options);
            //LogManager.Options = sp.GetService<ILogOptions>()!;

            //  try to update factory
            services.TryRegister(typeof(ILoggerFactory), () => LogManager.LoggerFactory);
            LogManager.LoggerFactory = sp.GetService<ILoggerFactory>()!;

            //  try to update message factory
            services.TryRegister(typeof(ILogMessageFactory), () => LogManager.MessageFactory);
            LogManager.MessageFactory = sp.GetService<ILogMessageFactory>()!;

            //  try to update message factory
            services.TryRegister(typeof(IExternalScopeProvider), () => LogManager.ScopeProvider);
            LogManager.ScopeProvider = sp.GetService<IExternalScopeProvider>()!;

            return services;
        }

        public static IProviderSetup AddLogging(this IProviderSetup services, Action<ILogOptions>? configure)
        {
            AddLogging(services);

            configure?.Invoke(LogManager.Options);

            return services;
        }

        /// <summary> Parsing the logging configuration from JSON </summary>
        /// <param name="options"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ILogOptions AddConfiguration(this ILogOptions options, JToken? config)
        {
            Throw.IsArgumentNullException(options, nameof(options));

            if (config != null)
            {
                if (config is JObject configObject)
                {
                    var p = configObject.Property("Logging", StringComparison.OrdinalIgnoreCase);
                    config = p?.Value ?? config;
                }

                foreach (JProperty jp in config.Children())
                {
                    if (jp.Name.Equals("LogLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load global category defaults
                        LoadRules(options.LogLevel, jp.Value);
                        continue;
                    }

                    if (jp.Value is JObject jo)
                    {
                        options.EnsureOptionItem(jp.Name).Populate(jo);
                        //// raw options of ILogObserver (including filterItem)
                        //var d = jo.ToObject<Dictionary<string, string>>();
                        //optionItem.Options.Merge(d);

                        //if (jo.GetValue("LogLevel", StringComparison.OrdinalIgnoreCase) is JToken jp2)
                        //{
                        //    // Load logger specific rules
                        //    LoadRules(optionItem.FilterItem, jp2);
                        //}
                    }
                }

                //  options already include filters
                options.AddDebug();
                options.AddConsole();
            }

            return options;

            void LoadRules(LoggerFilterItem filter, JToken configurationSection)
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(configurationSection.ToString(), filter);
                //var dd = configurationSection
                //    .ToObject<Dictionary<string, string?>>()!
                //    .ToDictionary(
                //        i => i.Key.Equals("Default", StringComparison.OrdinalIgnoreCase) ? string.Empty : i.Key,
                //        i => ParseLogLevel(i.Value));
                //foreach (var d in dd)
                //{
                //    filter.AddRule(d.Key, d.Value);
                //}
            }

            //static LogLevel ParseLogLevel(string? source)
            //{
            //    switch (source?.ToLowerInvariant())
            //    {
            //        case "trace": return LogLevel.Trace;
            //        case "debug": return LogLevel.Debug;
            //        case "info": return LogLevel.Info;
            //        case "warn": return LogLevel.Warning;
            //        case "error": return LogLevel.Error;
            //        case "critical": return LogLevel.Critical;
            //        //  MS
            //        case "information": return LogLevel.Info;
            //        case "warning": return LogLevel.Warning;
            //    };

            //    return LogLevel.None;
            //}
        }

        public static void Populate(this LogOptionItem item, JObject jo)
        {
            if (jo.TryGetValue(nameof(LogOptionItem.LogLevel), StringComparison.InvariantCultureIgnoreCase, out var jt))
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(jt.ToString(), item.LogLevel);
                jo.Remove(nameof(LogOptionItem.LogLevel));
            }

            Newtonsoft.Json.JsonConvert.PopulateObject(jo.ToString(), item.Options);
        }
    }
}