namespace Simple.Rest;

public interface IRequest
{
    string Host { get; }
    string Route { get; }
    string Method { get; }
    string? Content { get; }
    IDictionary<string, string> Headers { get; }
    IDictionary<string, string> Query { get; }
    string QueryString {  get; }


    IRequest SetContent(string content);

    IRequest SetHeader(string key, string? value);

    IRequest SetQuery(string key, string? value);

    /// <summary> Set hesder "Authorization" to $"Bearer {token}" </summary>
    /// <returns>self</returns>
    IRequest Token(string? token);
}

public class RestRequest : IRequest
{
    public RestRequest(string host, string route, string method, IDictionary<string, string>? headers)
    {
        Host = host;
        Route = route;
        Method = method;
        Headers = headers ?? new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Query = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public string Host { get; }
    public string Route { get; }
    public string Method { get; }
    public string? Content { get; set; }
    public IDictionary<string, string> Headers { get; set; }
    public IDictionary<string, string> Query { get; }

    public string QueryString => Query.Any()
        ? "?" + string.Join("&", Query.Select(i => $"{i.Key.Trim()}={i.Value.Trim()}"))
        : string.Empty;

    public IRequest SetContent(string body)
    {
        Content = body;
        return this;
    }

    public IRequest SetHeader(string key, string? value)
    {
        Headers[key] = value ?? string.Empty;
        return this;
    }

    public IRequest SetQuery(string key, string? value)
    {
        Query[key] = value ?? string.Empty;
        return this;
    }

    public IRequest Token(string? token)
        => SetHeader("Authorization", $"Bearer {token}");

    public override string ToString()
        => $"{Method} {Route}";
}