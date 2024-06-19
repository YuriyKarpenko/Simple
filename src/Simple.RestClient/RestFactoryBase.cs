namespace Simple.RestClient;

public interface IRestFactory
{
    IRequest CreateRequest(string route, string method, string? token = null);
    Task<IResponse> ExecuteAsync(IRequest req);
}

public class RestFactoryBase : IRestFactory
{
    protected readonly WebRunner _webRunner;
    protected readonly string _baseAddress;
    public RestFactoryBase(string baseAddress, WebRunner? customWebRunner = null)
    {
        _baseAddress = baseAddress;
        _webRunner = customWebRunner ?? new WebRunner();
        DefaultHeaders = new Dictionary<string, string>();
    }

    public virtual Dictionary<string, string> DefaultHeaders { get; }

    #region IRestFactory

    public virtual IRequest CreateRequest(string route, string method, string? token)
    {
        var headers = new Dictionary<string, string>(DefaultHeaders, StringComparer.InvariantCultureIgnoreCase);
        IRequest req = new RestRequest(_baseAddress, route, method, headers);
        return req.Token(token);
    }

    public virtual async Task<IResponse> ExecuteAsync(IRequest req)
    {
        req = OnEncrypt(req);

        var resp = await _webRunner.ExecuteAsync(req);

        resp = OnDecrypt(resp);

        return resp;
    }

    #endregion

    protected virtual IRequest OnEncrypt(IRequest req) => req;

    protected virtual IResponse OnDecrypt(IResponse resp) => resp;
}