namespace BlazorOAuth.API.Models.Entities;

public class User
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicture { get; set; }
    public int SignInCount { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresUtc { get; set; }
}
