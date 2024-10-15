using System.Runtime.InteropServices;

namespace Simple.Logging;

/// <summary>
/// "Implement" this interface in your class to get access to the Log()
/// Mixin, which will give you a Logger that includes the class name in the log.
/// </summary>
[ComVisible(false)]
public interface IEnableLogger
{
}