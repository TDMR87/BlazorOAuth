namespace BlazorOAuth.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController(ITokenService tokenService, IUserService userService) : ControllerBase
{
    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshAccessToken(
        [FromBody] string accessToken, 
        IOptions<RefreshTokenOptions> refreshTokenOptions, 
        CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(refreshTokenOptions.Value.CookieName, out var refreshToken))
        {
            return BadRequest();
        }

        var userIdResult = tokenService.GetUserId(accessToken);
        if (userIdResult.IsFailure)
        {
            return BadRequest();
        }

        var userResult = await userService.GetByIdAsync(userIdResult.Payload, cancellationToken);
        if (userResult.IsFailure)
        {
            return BadRequest();
        }

        var user = userResult.Payload;
        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiresUtc <= DateTime.UtcNow)
        {
            return BadRequest();
        }

        var accessTokenResult = tokenService.GenerateAccessToken(userResult.Payload);
        if (accessTokenResult.IsFailure)
        {
            return BadRequest();
        }

        return Ok(accessTokenResult.Payload);
    }
}
