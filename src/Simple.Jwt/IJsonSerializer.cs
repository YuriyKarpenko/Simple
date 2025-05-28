using Newtonsoft.Json;

namespace Simple.Jwt;

/// <summary>
/// Provides JSON Serialize and Deserialize. Allows custom serializers used.
/// </summary>
public interface IJsonSerializer
{
    /// <summary> Serializes an object to a JSON string. </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON string</returns>
    string Serialize(object obj);

    /// <summary> Deserializes a JSON string to an object of specified type. </summary>
    /// <param name="json">The JSON string deserialize.</param>
    /// <returns>Strongly-typed object.</returns>
    T Deserialize<T>(string? json);
}

public class NewtonsoftSerializer : IJsonSerializer
{
    private static readonly JsonSerializerSettings _serializeOptions = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    public string Serialize(object? obj)
    {
        Throw.IsArgumentNullException(obj, nameof(obj));

        return JsonConvert.SerializeObject(obj, _serializeOptions);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentException" />
    public T Deserialize<T>(string? json)
    {
        json ??= Throw.IsArgumentNullException(json, nameof(json));

        return JsonConvert.DeserializeObject<T>(json, _serializeOptions)!;
    }
}