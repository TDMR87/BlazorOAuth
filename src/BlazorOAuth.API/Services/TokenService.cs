namespace BlazorOAuth.API.Services;

public interface ITokenService
{
    Result<string> GenerateAccessToken(User user);
    Result<RefreshToken> GenerateRefreshToken(User user);
    Result<int> GetUserId(string accessToken);
}

internal class TokenService(IOptions<AuthorizationOptions> options) : ITokenService
{
    public Result<string> GenerateAccessToken(User user)
    {
        var config = options.Value.JwtOptions!;

        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new("name", user.Username),
            new("email", user.Email ?? string.Empty),
            new("profilepicture", user.ProfilePicture ?? string.Empty),
            new("signincount", user.SignInCount.ToString())
        };

        var signingKey = Encoding.UTF8.GetBytes(config.IssuerSigningKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = config.ValidIssuer,
            Audience = config.ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes((double)config.TokenLifetimeInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature),
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Result.Success(jwt);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(errorMessage: e.Message);
        }
    }

    public Result<RefreshToken> GenerateRefreshToken(User user)
    {
        var config = options.Value.RefreshTokenOptions!;

        return Result.Success(new RefreshToken
        (
            Value: Guid.NewGuid().ToString(),
            ExpiresUtc: DateTime.UtcNow.AddMinutes(config.TokenLifetimeInMinutes)
        ));
    }

    public Result<int> GetUserId(string accessToken)
    {
        var config = options.Value.JwtOptions!;

        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        if (token.Issuer != config.ValidIssuer)
        {
            Result.Failure<int>(errorMessage: "Invalid issuer");
        }

        var userId = token.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        return userId is not null
            ? Result.Success(int.Parse(userId))
            : Result.Failure<int>(errorMessage: accessToken);
    }
}