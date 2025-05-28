namespace Simple.Jwt;

public static class JwtErrors
{
    public const string ErrorTokenParse = "Token must consist of 3 delimited by dot parts.";
    public const string ErrorTokenUrlDecode = "Illegal base64url string.";
    //  validator
    public const string ErrorNoAlgorithm = "This instance was constructed without algorithm factory so cannot be used for signature validation";
    public const string ErrorInvalidSign = "The signature is invalid according to the validation procedure.";
    public const string ErrorInvalidNoSign = "Signature is not acceptable for algorithm None";
    public const string ErrorInvalidClaimExp = "Token has expired.";
    public const string ErrorInvalidClaimNbf = "Token is not yet valid.";

    public static string ErrorArgumentIsNull(string parameterName) => $"Argument is null: '{parameterName}'.";
    public static string ErrorArgumentIsInvalid(string parameterName) => $"Argument is invalid: '{parameterName}'.";

    public static string ErrorTimeClaim(string parameterName) => $"Claim '{parameterName}' must be a number.";

    public static bool IsArgumentNotNull(object? value, string paramName, out string? error)
    {
        error = value is null ? ErrorArgumentIsNull(paramName) : null;
        return error is null;
    }

    public static bool IsArgumentValid<T>(T value, Predicate<T> isValid, string paramName, out string? error)
    {
        error = !isValid(value) ? ErrorArgumentIsInvalid(paramName) : null;
        return error is null;
    }

    public static bool IsArgumentNotEmpty<T>(T? value, Predicate<T> isValid, string paramName, out string? error)
        => IsArgumentNotNull(value, paramName, out error) && IsArgumentValid(value!, isValid, paramName, out error);
}