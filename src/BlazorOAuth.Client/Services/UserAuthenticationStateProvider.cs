namespace BlazorOAuth.Client.Services;

public class UserAuthenticationStateProvider(BrowserStorageService storageService, TokenService tokenService) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal AnonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
    public CurrentUser CurrentUser { get; private set; } = new();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claimsPrincipal = await GetClaimsPrincipal();
        UpdateCurrentUser(claimsPrincipal);
        return new AuthenticationState(claimsPrincipal);
    }

    private async Task<ClaimsPrincipal> GetClaimsPrincipal()
    {
        var tokenFromStorage = await storageService.GetAccessToken();
        if (string.IsNullOrWhiteSpace(tokenFromStorage))
        {
            CurrentUser = new CurrentUser();
            return AnonymousUser;
        }

        var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenFromStorage);
        if (accessToken.ValidTo < DateTime.UtcNow)
        {
            var refreshedToken = await tokenService.RefreshAccessToken(tokenFromStorage);
            if (refreshedToken is null)
            {
                CurrentUser = new();
                return AnonymousUser;
            }

            await storageService.SaveAccessToken(refreshedToken);
        }

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(accessToken.Claims, "BlazorOAuth"));
        UpdateCurrentUser(claimsPrincipal);
        return claimsPrincipal;
    }

    public async Task NotifyUserLoggedin()
    {
        var principal = await GetClaimsPrincipal();
        var authState = Task.FromResult(new AuthenticationState(principal));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task NotifyUserLoggedOut()
    {
        await storageService.RemoveAccessToken();
        var authState = Task.FromResult(new AuthenticationState(AnonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private void UpdateCurrentUser(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            CurrentUser = new()
            {
                IsAuthenticated = true,
                Id = int.Parse(claimsPrincipal.FindFirst(c => c.Type == "id")?.Value!),
                Username = claimsPrincipal.FindFirst(c => c.Type == "name")?.Value,
                Email = claimsPrincipal.FindFirst(c => c.Type == "email")?.Value,
                ProfilePicture = claimsPrincipal.FindFirst(c => c.Type == "profilepicture")?.Value,
                SignInCount = int.Parse(claimsPrincipal.FindFirst(c => c.Type == "signincount")?.Value!)
            };
        }
        else
        {
            CurrentUser = new();
        }
    }
}

public class CurrentUser
{
    public int? Id { get; init; } = null;
    public bool IsAuthenticated { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? ProfilePicture { get; init; }
    public int SignInCount { get; set; }
    public string GetSignInCountOrdinal()
    {
        if (SignInCount <= 0) return string.Empty;

        return (SignInCount % 100) switch
        {
            11 or 12 or 13 => SignInCount + "th",
            _ => (SignInCount % 10) switch
            {
                1 => SignInCount + "st",
                2 => SignInCount + "nd",
                3 => SignInCount + "rd",
                _ => SignInCount + "th",
            },
        };
    }
}