﻿using System;

namespace Simple.Logging
{
    public interface ILoggingBus<TLogMessage> : IObservable<TLogMessage>
    {
        /// <summary> Eter new message for observers </summary>
        void Push(TLogMessage entry);
    }
}