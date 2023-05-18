using System;

namespace Simple.Logging.Messages
{
    public class LogMessageString : LogMessageBase
    {
        public LogMessageString(Type logSource, LogLevel level, string message, Exception? exception) : base(logSource, level, exception)
        {
            Message = message;
        }

        public string Message { get; set; }

        public override string ToString()
        {
            var end = Exception == null ? string.Empty : $"\n {Exception}";
            return $"{Level,-8} - {Name,-10} : {Message} {end}\n";
        }
    }
}
