using System;

using Simple.Logging.Messages;

namespace Simple.Logging.Observers
{
    [LoggerName("Console")]
    public class ObserverConsole : ObserverBase<ObserverConsole>
    {
        protected override void Write(ILogMessage e)
        {
            Console.WriteLine(e);
        }
    }
}