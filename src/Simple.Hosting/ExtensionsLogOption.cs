using System.Linq;

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
        public static ILogOptions AddConfiguration(this ILogOptions options, JToken? configToken)
        {
            Throw.IsArgumentNullException(options, nameof(options));

            if (configToken is JObject config)
            {
                //if (config)
                {
                    var p = config.Property("Logging", StringComparison.OrdinalIgnoreCase);
                    config = p?.Value as JObject ?? config;
                }

                // try load global category defaults
                TryLoadRules(options.LogLevel, config, nameof(ILogOptions.LogLevel));

                //Newtonsoft.Json.JsonConvert.PopulateObject(config.ToString(), options.RawOptions);

                foreach (JProperty jp in config.Properties())
                {
                    if (jp.Value is JObject jo)
                    {
                        //  fill other configurations
                        var ro = new LogOptionItemRaw(options.LogLevel.Default);
                        ro.Populate(jo);
                        options.RawOptions[jp.Name] = ro;

                        //if (jp.Name.Equals(LogOptionItemConsole.SConfigName, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    options.AddConsole(loi => loi.Populate(jo));
                        //    config.Remove(jp.Name);
                        //    continue;
                        //}

                        //if (jp.Name.Equals(LogOptionItemDebug.SConfigName, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    options.AddDebug(loi => loi.Populate(jo));
                        //    config.Remove(jp.Name);
                        //    continue;
                        //}

                        //options.EnsureOptionItem(new LogOptionItem(jp.Name)).Populate(jo);
                    }
                }

                options.AddConsole();
                options.AddDebug();
            }

            return options;
        }

        static bool TryLoadRules(LoggerFilterItem filter, JToken? jtoken, string filterName = nameof(ILogOptionItem.LogLevel))
        {
            if (jtoken is JObject jo && jo.TryGetValue(filterName, StringComparison.OrdinalIgnoreCase, out var jt))
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(jt.ToString(), filter);
                jo.Remove(filterName);
                return true;
            }

            return false;
        }

        public static void Populate(this ILogOptionItem item, JObject jo)
        {
            TryLoadRules(item.LogLevel, jo);

            Newtonsoft.Json.JsonConvert.PopulateObject(jo.ToString(), item);
        }
    }
}