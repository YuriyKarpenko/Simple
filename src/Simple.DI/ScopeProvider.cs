using System;
using System.Threading;

namespace Simple.DI;

public class ScopeProvider<T> where T : class
{
    private readonly AsyncLocal<Scope?> _currentScope = new();

    public T? Current => _currentScope.Value?.State;

    public IDisposable Push(T state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var parent = _currentScope.Value;
        var newScope = new Scope(this, state, parent);
        _currentScope.Value = newScope;

        return newScope;
    }

    private sealed class Scope(ScopeProvider<T> provider, T state, Scope? parent) : IDisposable
    {
        private bool _isDisposed;

        public Scope? Parent => parent;
        public T State => state;


        public void Dispose()
        {
            if (!_isDisposed)
            {
                provider._currentScope.Value = Parent;
                _isDisposed = true;
            }
        }
    }
}