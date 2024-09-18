using BlazorOAuth.API.Models.Contracts;

namespace BlazorOAuth.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    IAuthService authService, 
    IOptions<AuthenticationOptions> authenticationOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions) : ControllerBase
{
    [HttpGet]
    [Route("signin/google")]
    [AllowAnonymous]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SigninWithGoogle()
    {
        if (!TryGetAuthorizationToken(Request.Headers, out var authorizationToken))
        {
            return Unauthorized();
        }

        var authenticationResult = await authService.AuthenticateWithGoogleAsync(authorizationToken!);
        if (authenticationResult.IsFailure)
        {
            return Unauthorized();
        }

        var signInResult = await authService.SignInAsync(authenticationResult.Payload);
        if (signInResult.IsFailure)
        {
            return Unauthorized();
        }

        return SignedInUser(signInResult);
    }

    [HttpGet]
    [Route("redirect/google")]
    [AllowAnonymous]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    public IActionResult GetGoogleRedirect([FromQuery] string state)
    {
        var config = authenticationOptions.Value.GoogleOptions!;

        var googleAuthUrl =
            $"{config.AuthenticationEndpoint}?" +
            $"response_type={config.ResponseType}&" +
            $"client_id={config.ClientId}&" +
            $"redirect_uri={config.RedirectUri}&" +
            $"scope={config.Scope}&" +
            $"state={state}&" +
            $"nonce={Guid.NewGuid()}";

        return Ok(googleAuthUrl);
    }

    [NonAction]
    private IActionResult SignedInUser(Result<SignInResponse> signInResult)
    {
        var config = refreshTokenOptions.Value;
        var accessToken = signInResult.Payload.AccessToken;
        var refreshToken = signInResult.Payload.RefreshToken;

        Response.Cookies.Append(config.CookieName, refreshToken.Value, new()
        {
            Secure = true,
            HttpOnly = true,
            Path = "/api/token/refresh",
            SameSite = SameSiteMode.None,
            Expires = refreshToken.ExpiresUtc
        });

        return signInResult.IsSuccess
            ? Ok(accessToken)
            : Unauthorized();
    }

    private static bool TryGetAuthorizationToken(IHeaderDictionary requestHeaders, out string? authorizationToken)
    {
        authorizationToken = null;

        string? authorizationHeaderValue = requestHeaders.Authorization;
        if (authorizationHeaderValue is null)
        {
            return false;
        }

        if (!authorizationHeaderValue.StartsWith("bearer", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        authorizationToken = authorizationHeaderValue.Remove(0, "bearer".Length).Trim();
        return true;
    }
}
