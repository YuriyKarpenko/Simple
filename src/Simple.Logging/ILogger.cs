using System;

using Simple.Logging.Messages;

namespace Simple.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Only for compatibility with <see cref="Microsoft.Extensions.Logging"/>
        /// </summary>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="string"/> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        ILogMessage? Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string>? formatter = null);

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
        IDisposable BeginScope<TState>(TState state) where TState : notnull;
    }
}