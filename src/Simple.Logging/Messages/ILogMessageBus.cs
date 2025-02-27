using System;

namespace Simple.Logging.Messages;

public interface ILogMessageBus<TLogMessage> : IObservable<TLogMessage>
{
    /// <summary> Eter new message for observers </summary>
    void Push(TLogMessage entry);
}