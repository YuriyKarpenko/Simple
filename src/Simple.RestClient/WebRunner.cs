using System.Text;

namespace Simple.RestClient;

/// <summary> For customize behavior we can use OR action-property OR override corresponding methods in inheritance-class (inheritance has more priority) </summary>
public class WebRunner
{
    public WebRunner()
    {
        RequestTimeout = TimeSpan.FromSeconds(5);
    }

    /// <summary> Timeout for HttpClient (actual only when ClientFactory==null) </summary>
    public TimeSpan RequestTimeout { get; set; }
    public Func<HttpClient>? ClientFactory { get; set; }
    public Func<IRequest, Task<HttpRequestMessage>>? RequestFactoryAsync { get; set; }
    public Func<IRequest, HttpResponseMessage, Task<IResponse>>? ResponseFactoryAsync { get; set; }


    public async Task<IResponse> ExecuteAsync(IRequest request)
    {
        var req = await MapRequestAsync(request);

        using var client = CreateClient();

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

        var response = await MapResponseAsync(request, resp).ConfigureAwait(false);
        return response;
    }


    protected virtual Task<HttpRequestMessage> MapRequestAsync(IRequest request)
    {
        if (RequestFactoryAsync != null)
        {
            return RequestFactoryAsync(request);
        }

        var baseUri = new Uri(request.Host);
        if (Uri.TryCreate(baseUri, request.Route + request.QueryString, out var absoluteUri))
        {
            var method = new HttpMethod(request.Method);
            var req = new HttpRequestMessage(method, absoluteUri);

            foreach (var h in request.Headers)
            {
                req.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
            req.Headers.AcceptCharset.TryParseAdd(Encoding.UTF8.BodyName);
            req.Headers.AcceptEncoding.TryParseAdd(Encoding.UTF8.BodyName);

            if (!string.IsNullOrEmpty(request.Content))
            {
                req.Content = new StringContent(request.Content, Encoding.UTF8, "application/json");
            }

            return Task.FromResult(req);
        }

        throw new InvalidCastException($"wrong request: {request}");
    }

    protected virtual HttpClient CreateClient()
        => ClientFactory == null
            ? new HttpClient() { Timeout = RequestTimeout }
            : ClientFactory();

    protected virtual async Task<IResponse> MapResponseAsync(IRequest request, HttpResponseMessage message)
    {
        if (ResponseFactoryAsync != null)
        {
            return await ResponseFactoryAsync(request, message);
        }

        var content = await message.Content.ReadAsStringAsync();
        var headers = message.Headers.Concat(message.Content.Headers)
            .ToDictionary(i => i.Key, i => string.Join(", ", i.Value));

        var res = new RestResponse(
            request,
            message.StatusCode,
            content,
            headers
        );

        return res;
    }
}