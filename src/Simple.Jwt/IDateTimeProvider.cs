namespace Simple.Jwt;

/// <summary>
/// Represents a DateTime provider.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current DateTime.
    /// </summary>
    DateTimeOffset GetNow();
}

/// <summary> Provider for UTC DateTime. </summary>
public sealed class UtcDateTimeProvider : IDateTimeProvider
{
    /// <summary> Retuns the current time (UTC). </summary>
    public DateTimeOffset GetNow()
    {
        return DateTimeOffset.UtcNow;
    }
}