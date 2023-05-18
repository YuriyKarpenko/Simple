namespace Simple.Configuration.Sources
{
    /// <summary>
    /// Represents a chained IConfiguration as an <see cref="IConfigurationSource"/>.
    /// </summary>
    internal class ChainedSource : IConfigurationSource
    {
        private readonly IConfiguration _configuration;
        public ChainedSource(IConfiguration config)
        {
            _configuration = config;
        }


        /// <inheritdoc />
        public IConfiguration Build(IConfigurationBuilder builder, IConfiguration config)
        {
            foreach (var key in _configuration.Keys)
            {
                config[key] = _configuration[key];
            }
            return config;
        }
    }
}
