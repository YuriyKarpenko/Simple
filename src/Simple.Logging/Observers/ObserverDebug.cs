using System;
using System.Diagnostics;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers;

/// <summary> A logger that writes messages in the debug output window only when a debugger is attached. </summary>
[LoggerName("Debug")]
public class ObserverDebug : ObserverBase<ObserverDebug>
{
    public ObserverDebug(ILogOptions options, Action<LoggerFilterItem>? configure = null) : base(options)
    {
        configure?.Invoke(FilterItem);
    }


    protected override void Write(LogMessage message)
    {
        //return Debugger.IsAttached ?
        Debug.Write($"{message.Level,-9}");
        Debug.WriteLine(LogManager.MessageFactory.ToStringWithoutLevel(message, false));
    }
}