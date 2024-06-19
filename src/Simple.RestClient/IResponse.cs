using System.Net;

namespace Simple.RestClient;

public interface IResponse
{
    IRequest Request { get; }
    HttpStatusCode StatusCode { get; }
    string? Content { get; }
    IDictionary<string, string> Headers { get; }

    IResponse SetContent(string? content);
}

public class RestResponse : IResponse
{
    public IRequest Request { get; }
    public HttpStatusCode StatusCode { get; }
    public string? Content { get; protected set; }
    public IDictionary<string, string> Headers { get; }


    public RestResponse(IRequest request, HttpStatusCode statusCode, string? content, IDictionary<string, string> headers)
    {
        Request = request;
        StatusCode = statusCode;
        Content = content;
        Headers = headers;
    }

    IResponse IResponse.SetContent(string? content)
    {
        Content = content;
        return this;
    }

    public override string ToString()
        => $"{(int)StatusCode} ({StatusCode}) '{Content}'";
}