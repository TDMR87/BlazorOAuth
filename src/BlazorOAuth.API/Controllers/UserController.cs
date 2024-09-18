namespace BlazorOAuth.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(ITokenService tokenService, IUserService userService) : ControllerBase
{
    [HttpDelete]
    [Route("")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(CancellationToken cancellationToken)
    {
        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        var accessToken = authHeader?.Remove(0, "bearer".Length).Trim();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized();
        }

        var userIdResult = tokenService.GetUserId(accessToken);
        if (userIdResult.IsFailure)
        {
            return BadRequest();
        }

        var deleteUserResult = await userService.DeleteAsync(userIdResult.Payload, cancellationToken);
        if (deleteUserResult.IsFailure)
        {
            return Problem(statusCode: 500);
        }

        return NoContent();
    }
}
