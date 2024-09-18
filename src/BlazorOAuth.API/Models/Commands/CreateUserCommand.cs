namespace BlazorOAuth.API.Models.Commands;

public sealed class CreateUserCommand
{
    public required string ExternalId { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicture { get; set; }
}
