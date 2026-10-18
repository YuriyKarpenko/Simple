using System;
using System.Collections.Generic;
using System.Threading;

namespace Simple.DI;

/// <summary> AsyncLocal stack of scopes: values flow with the async execution context; the current scope owns its tracked disposables </summary>
public class ScopeProvider<T> where T : class
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    /// <summary> State of the current scope, or null when no scope is active </summary>
    public T? Current => _currentScope.Value?.State;

    /// <summary> Current scope object (state + tracked disposables), or null when no scope is active </summary>
    internal Scope? CurrentScope => _currentScope.Value;

    /// <summary> Pushes a new scope onto the stack; Dispose unwinds it and disposes tracked disposables in reverse creation order </summary>
    public IDisposable Push(T state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var parent   = _currentScope.Value;
        var newScope = new Scope(this, state, parent);
        _currentScope.Value = newScope;

        return newScope;
    }

    internal sealed class Scope(ScopeProvider<T> provider, T state, Scope? parent) : IDisposable
    {
        private List<IDisposable>? _disposables;
        private bool               _isDisposed;

        public Scope? Parent => parent;
        public T State => state;

        public void Track(object? instance)
        {
            if (_isDisposed)
            {
                return;
            }
            if (instance is IDisposable disposable)
            {
                var list = _disposables ??= new List<IDisposable>();
                if (!list.Contains(disposable))
                {
                    list.Add(disposable);
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            var cur = provider._currentScope.Value;
            while (cur != null && !ReferenceEquals(cur, this))
            {
                cur = cur.Parent;
            }
            if (cur != null)
            {
                provider._currentScope.Value = parent;
            }

            var list = _disposables;
            _disposables = null;
            if (list != null)
            {
                List<Exception>? errors = null;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        list[i].Dispose();
                    }
                    catch (Exception ex)
                    {
                        (errors ??= new()).Add(ex);
                    }
                }
                if (errors != null)
                {
                    throw new AggregateException(errors);
                }
            }

            if (state is IDisposable services)
            {
                services.Dispose();
            }
        }
    }
}