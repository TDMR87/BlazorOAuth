namespace BlazorOAuth.API.Models.Contracts;

public sealed record AuthResponse(
    string ExternalId,
    string Username,
    string Email,
    string? ProfilePicture
);
