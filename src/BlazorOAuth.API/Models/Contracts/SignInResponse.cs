namespace BlazorOAuth.API.Models.Contracts;

public sealed record SignInResponse(
    string AccessToken,
    RefreshToken RefreshToken
);