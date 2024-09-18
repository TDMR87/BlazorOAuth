var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IdentityProviderService>();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<UserAuthenticationStateProvider>());
builder.Services.AddTransient<CookieDelegatingHandler>();
builder.Services.AddTransient<TokenDelegatingHandler>();

builder.Services.AddHttpClient("", (serviceProvider, httpClient) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    httpClient.BaseAddress = new Uri(configuration["ApiBaseAddress"]!);
}).AddHttpMessageHandler<CookieDelegatingHandler>()
.AddHttpMessageHandler<TokenDelegatingHandler>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();