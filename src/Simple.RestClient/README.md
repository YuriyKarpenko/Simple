# Simple.Rest

How often did we create a "bicycle" around Httpclient? This is the following))

##	Using
```cs
    public RestFactory() : base("http://SomeServerHost.dom:1111")
    {
        base.DefaultHeaders = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                ...,
            };
        ...
    }
```
OR
```cs
    private static readonly System.Security.Cryptography.X509Certificates.X509Certificate _cert = CreateCert();

    private static System.Security.Cryptography.X509Certificates.X509Certificate CreateCert(){...}

    private static HttpClientHandler CreateHttpsClientHandler()
    {
        var h = new HttpClientHandler();
        ...
        return h;
    }

    private static HttpClient CreateClient()
        => new HttpClient(CreateHttpsClientHandler());

    private static readonly Dictionary<string, string> _defaultHeaders =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Accept", "application/json" }, 
            { "Content-Type", "application/json" }, 
            ...,
        };

    public RestFactory(SomeConfig? config) : base(config.SomeServerHost)
    {
        base._webRunner.ClientFactory = CreateClient;
        base.DefaultHeaders = _defaultHeaders;
        ...
    }

```
##  Release notes
### 0.1.0
	start project