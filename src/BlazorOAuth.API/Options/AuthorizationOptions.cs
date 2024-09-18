namespace BlazorOAuth.API.Options;

public sealed class AuthorizationOptions
{
    public JwtOptions? JwtOptions { get; set; }
    public RefreshTokenOptions? RefreshTokenOptions { get; set; }
}

public sealed class JwtOptions
{
    public required string ValidIssuer { get; set; }
    public required string ValidAudience { get; set; }
    public required string IssuerSigningKey { get; set; }
    public required int TokenLifetimeInMinutes { get; set; }
    public required bool ValidateLifetime { get; set; }
    public required bool ValidateIssuer { get; set; }
    public required bool ValidateAudience { get; set; }
    public required bool ValidateSigningKey { get; set; }
}

public sealed class RefreshTokenOptions
{
    public required string CookieName { get; set; }
    public required int TokenLifetimeInMinutes { get; set; }
}
