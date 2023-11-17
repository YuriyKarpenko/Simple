using System;

using Simple.Helpers;

namespace Simple.Logging.Observers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class LoggerNameAttribute : Attribute
    {
        public LoggerNameAttribute(string name)
        {
            Name = Throw.IsArgumentNullException(name, nameof(name));
        }

        public string Name { get; }
    }
}
