namespace Simple.Jwt;

public static class UnixEpoch
{
    /// <summary>
    /// Describes a point in time, defined as the number of seconds that have elapsed since 00:00:00 UTC, Thursday, 1 January 1970, not counting leap seconds.
    /// See https://en.wikipedia.org/wiki/Unix_time />
    /// </summary>
    public static DateTime Value { get; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Per JWT spec:
    /// Gets the number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the desired date/time.
    /// </summary>
    /// <param name="datetime">The DateTime to convert to seconds.</param>
    /// <remarks>if dateTimeUtc less than UnixEpoch, return 0</remarks>
    /// <returns>the number of seconds since Unix Epoch.</returns>
    public static long GetSecondsSince(DateTimeOffset time)
    {
        var dateTimeUtc = time.ToUniversalTime();

        return dateTimeUtc <= Value
            ? 0
            : (long)(dateTimeUtc - Value).TotalSeconds;
    }
}