﻿namespace Simple.Hosting;

public static class ExtensionsHostEnv
{
    /// <summary>
    /// Checks if the current hosting environment name is <see cref="EnvironmentName.Development"/>.
    /// </summary>
    /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
    /// <returns>True if the environment name is <see cref="EnvironmentName.Development"/>, otherwise false.</returns>
    public static bool IsDevelopment(this IHostingEnvironment hostingEnvironment)
        => string.Equals(hostingEnvironment.Ensure().EnvironmentName, EnvironmentNames.Development, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if the current hosting environment name is <see cref="EnvironmentName.Production"/>.
    /// </summary>
    /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
    /// <returns>True if the environment name is <see cref="EnvironmentName.Production"/>, otherwise false.</returns>
    public static bool IsProduction(this IHostingEnvironment hostingEnvironment)
        => string.Equals(hostingEnvironment.Ensure().EnvironmentName, EnvironmentNames.Production, StringComparison.OrdinalIgnoreCase);


    private static IHostingEnvironment Ensure(this IHostingEnvironment env)
        => Throw.IsArgumentNullException(env, nameof(env));
}