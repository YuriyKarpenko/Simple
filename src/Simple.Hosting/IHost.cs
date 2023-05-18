using System;

namespace Simple.Hosting
{
    public interface IHost : IDisposable
    {
        /// <summary> The <see cref="IServiceProvider"/> for the host </summary>
        IServiceProvider Services { get; }

        /// <summary> Starts application </summary>
        void Start(Action entryPoint);
    }
}
