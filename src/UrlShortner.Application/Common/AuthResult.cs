// src/UrlShortner.Application/Common/AuthResult.cs
namespace UrlShortner.Application.Common;

public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    private AuthResult() { }

    public static AuthResult Success(string message, string? accessToken = null, string? refreshToken = null)
    {
        return new AuthResult
        {
            IsSuccess = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public static AuthResult Failure(string message)
    {
        return new AuthResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}