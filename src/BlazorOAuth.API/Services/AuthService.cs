namespace BlazorOAuth.API.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(string authorizationCode, CancellationToken cancellationToken = default);
    Task<Result<SignInResponse>> SignInAsync(AuthResponse AuthResponse, CancellationToken cancellationToken = default);
}

internal sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<AuthenticationOptions> options,
    IUserService userService,
    ITokenService tokenService) : IAuthService
{
    public async Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(string authorizationCode, CancellationToken cancellationToken)
    {
        var config = options.Value.GoogleOptions!;

        var idTokenRequestContent = new FormUrlEncodedContent
        ([
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("client_id", config.ClientId),
            new KeyValuePair<string, string>("client_secret", config.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        ]);

        // Exchange the Google authorization code for access token
        var authorizationCodeExchangeRequest = await httpClientFactory.CreateClient().PostAsync(
            config.AuthorizationCodeEndpoint, idTokenRequestContent, cancellationToken);

        if (!authorizationCodeExchangeRequest.IsSuccessStatusCode)
        {
            var responseMessage = await authorizationCodeExchangeRequest.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<AuthResponse>(errorMessage: $"Authorization code exchange failed. {responseMessage}");
        }

        var idTokenContent = await authorizationCodeExchangeRequest.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);
        if (idTokenContent?.id_token is null)
        {
            return Result.Failure<AuthResponse>(errorMessage: "id_token not found in the response from the identity provider");
        }

        try
        {
            var validatedUser = await GoogleJsonWebSignature.ValidateAsync
            (
                validationSettings: new() { Audience = [config.ClientId] },
                jwt: idTokenContent?.id_token
            );

            return Result.Success(new AuthResponse
            (
                ProfilePicture: validatedUser.Picture,
                ExternalId: validatedUser.Subject,
                Username: validatedUser.Name,
                Email: validatedUser.Email
            ));
        }
        catch (InvalidJwtException e)
        {
            return Result.Failure<AuthResponse>(errorMessage: $"The id_token did not pass validation. {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Failure<AuthResponse>(errorMessage: $"Encountered an error during id_token validation. {e.Message}");
        }
    }

    public async Task<Result<SignInResponse>> SignInAsync(AuthResponse AuthResponse, CancellationToken cancellationToken = default)
    {
        User appUser;

        var existingUser = await userService.GetByExternalIdAsync(AuthResponse.ExternalId, cancellationToken);
        if (existingUser.IsSuccess)
        {
            appUser = existingUser.Payload;

            if (UserInfoChanged(currentInfo: appUser, newInfo: AuthResponse))
            {
                appUser.ProfilePicture = AuthResponse.ProfilePicture;
                appUser.Username = AuthResponse.Username;
                appUser.Email = AuthResponse.Email;
                await userService.UpdateAsync(appUser, CancellationToken.None);
            }
        }
        else
        {
            var createUser = await userService.CreateAsync(new()
            {
                ProfilePicture = AuthResponse.ProfilePicture,
                ExternalId = AuthResponse.ExternalId,
                Username = AuthResponse.Username,
                Email = AuthResponse.Email,
            }, CancellationToken.None);

            if (createUser.IsSuccess)
            {
                appUser = createUser.Payload;
            }
            else
            {
                return Result.Failure<SignInResponse>(errorMessage: createUser.Message);
            }
        }

        var accessTokenResult = tokenService.GenerateAccessToken(appUser);
        if (accessTokenResult.IsFailure)
            return Result.Failure<SignInResponse>(errorMessage: accessTokenResult.Message);

        var refreshTokenResult = tokenService.GenerateRefreshToken(appUser);
        if (refreshTokenResult.IsFailure)
            return Result.Failure<SignInResponse>(errorMessage: refreshTokenResult.Message);

        appUser.RefreshToken = refreshTokenResult.Payload.Value;
        appUser.RefreshTokenExpiresUtc = refreshTokenResult.Payload.ExpiresUtc;
        appUser.SignInCount++;

        await userService.UpdateAsync(appUser, CancellationToken.None);
        return Result.Success(new SignInResponse(accessTokenResult.Payload, refreshTokenResult.Payload));
    }

    private bool UserInfoChanged(User currentInfo, AuthResponse newInfo) => 
        currentInfo.Username != newInfo.Username || 
        currentInfo.Email != newInfo.Email ||
        currentInfo.ProfilePicture != newInfo.ProfilePicture;
}
