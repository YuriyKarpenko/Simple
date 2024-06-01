namespace Simple.Rest;

public interface IRestFactory
{
    IRequest CreateRequest(string route, string method, string? token);
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

    public Dictionary<string, string> DefaultHeaders { get; set; }

    #region IRestFactory

    public IRequest CreateRequest(string route, string method, string? token)
    {
        IRequest req = new RestRequest(_baseAddress, route, method, new Dictionary<string, string>(DefaultHeaders));
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

    //  TODO: encrypt
    protected virtual IRequest OnEncrypt(IRequest req) => req;

    //  TODO: decrypt
    protected virtual IResponse OnDecrypt(IResponse resp) => resp;
}