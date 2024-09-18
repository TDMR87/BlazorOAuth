namespace BlazorOAuth.Client.Services.Http;

/// <summary>
/// Handles attaching our API's access token to outgoing HTTP requests
/// and also handles refreshing of that token.
/// </summary>
/// <param name="browserStorage"></param>
/// <param name="tokenService"></param>
public class TokenDelegatingHandler(BrowserStorageService browserStorage, TokenService tokenService) : DelegatingHandlerBase
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the stored access token for our API
        var accessToken = await browserStorage.GetAccessToken();

        if (ShouldHandle(request) && !string.IsNullOrWhiteSpace(accessToken))
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

            // If access token has expired, refresh it
            if (token.ValidTo <= DateTime.UtcNow)
            {
                accessToken = await tokenService.RefreshAccessToken(accessToken, cancellationToken);
                if (accessToken is not null)
                {
                    await browserStorage.SaveAccessToken(accessToken);
                }
            }

            // Attach the access token to the request
            request.Headers.Authorization = new("Bearer", accessToken);
        }

        // Continue sending the request
        return await base.SendAsync(request, cancellationToken);
    }
}
