using System;
using System.Collections.Generic;
using System.Threading;

namespace Simple.Logging.Scope;

public class DefaultScopeProvider : IExternalScopeProvider
{
    private readonly AsyncLocal<Scope?> _currentScope = new AsyncLocal<Scope?>();


    /// <inheritdoc />
    public void ForEachScope<TState>(Action<object, TState> callback, TState state)
    {
        if (_currentScope.Value != null)
        {
            foreach (var scoe in GetScopes(_currentScope.Value))
            {
                callback(scoe, state);
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Push(object? state)
    {
        var parent = _currentScope.Value;
        var newScope = new Scope(this, Throw.IsArgumentNullException(state, nameof(state)), parent);
        _currentScope.Value = newScope;

        return newScope;
    }

    /// <summary> Get all scopes states (from root to current) </summary>
    /// <param name="scope">Current scope</param>
    /// <returns>All states of scopes</returns>
    private static IEnumerable<object> GetScopes(Scope scope)
    {
        if (scope.Parent != null)
        {
            yield return GetScopes(scope.Parent);
        }
        yield return scope.State;
    }

    private sealed class Scope : IDisposable
    {
        private readonly DefaultScopeProvider _provider;
        private bool _isDisposed;

        internal Scope(DefaultScopeProvider provider, object state, Scope? parent)
        {
            _provider = provider;
            State = state;
            Parent = parent;
        }

        public Scope? Parent { get; }

        public object State { get; }

        public override string? ToString()
            => State?.ToString();

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _provider._currentScope.Value = Parent;
                _isDisposed = true;
            }
        }
    }
}