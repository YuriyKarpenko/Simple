﻿using System;

namespace Simple.Logging.Scope
{
    /// <summary>
    /// Represents a storage of common scope data.
    /// </summary>
    public interface IScopeProvider
    {
        /// <summary>
        /// Executes callback for each currently active scope objects in order of creation.
        /// All callbacks are guaranteed to be called inline from this method.
        /// </summary>
        /// <param name="callback">The callback to be executed for every scope object</param>
        /// <param name="state">The state object to be passed into the callback</param>
        /// <typeparam name="TState">The type of state to accept.</typeparam>
        void ForEachScope<TState>(Action<object, TState> callback, TState state);

        /// <summary>
        /// Adds scope object to the list
        /// </summary>
        /// <param name="state">The scope object</param>
        /// <returns>The <see cref="IDisposable"/> token that removes scope on dispose.</returns>
        IDisposable Push(object state);

        object[] Scopes();
    }
}