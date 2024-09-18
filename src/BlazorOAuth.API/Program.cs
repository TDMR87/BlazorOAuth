var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<AuthorizationOptions>(builder.Configuration.GetSection("Authorization"));
builder.Services.Configure<RefreshTokenOptions>(builder.Configuration.GetSection("Authorization:RefreshTokenOptions"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .WithOrigins("https://localhost:5001", "https://blazoroauth.azurewebsites.net")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("Authorization:JwtOptions").Get<JwtOptions>()!;
    options.TokenValidationParameters = new()
    {
        ValidIssuer = jwtOptions.ValidIssuer,
        ValidAudience = jwtOptions.ValidAudience,
        ValidateIssuer = jwtOptions.ValidateIssuer,
        ValidateLifetime = jwtOptions.ValidateLifetime,
        ValidateAudience = jwtOptions.ValidateAudience,
        ValidateIssuerSigningKey = jwtOptions.ValidateSigningKey,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey!))
    };
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }

    app.Run();
}
catch (Exception e)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError("Database migration error. {error}", e.ToString());
}