namespace Simple.Hosting
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public HostingEnvironment(string envName, string appName, string rootPath)
        {
            EnvironmentName = envName;
            ApplicationName = appName;
            ContentRootPath = rootPath;
        }

        /// <inheritdoc />
        public string EnvironmentName { get; set; }
        /// <inheritdoc />
        public string ApplicationName { get; set; }
        /// <inheritdoc />
        public string ContentRootPath { get; set; }
    }
}
