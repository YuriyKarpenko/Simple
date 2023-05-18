namespace Simple.Configuration.Sources
{
    /// <summary> Represents a source of configuration key/values for an application. </summary>
    public interface IConfigurationSource
    {
        /// <summary> Builds the <see cref="IConfigurationBuilder"/> for this source. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>An <see cref="IConfiguration"/></returns>
        IConfiguration Build(IConfigurationBuilder builder, IConfiguration config);
    }
}