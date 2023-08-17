using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers
{
    [LoggerName("Console")]
    public class ObserverConsole : ObserverBase<ObserverConsole>
    {
        public ObserverConsole(ILogOptions options) : base(options) { }


        protected override void Write(ILogMessage e)
        {
            Console.WriteLine(e);
        }
    }
}