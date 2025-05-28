namespace Tests.Jwt;
public class BaseTests : TokenParameters
{
    protected JwtHeader _header = new JwtHeader();

    protected BaseTests()
    {
        _header.typ = JwtHeader.JwtType;
    }
}