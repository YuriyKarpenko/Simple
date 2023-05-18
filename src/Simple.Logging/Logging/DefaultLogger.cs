using System;

namespace Simple.Logging.Logging
{
    public class DefaultLogger : LoggerBase//<LogMessageString>
    {
        public DefaultLogger(Type logSoutce) : base(logSoutce) { }
    }

}
