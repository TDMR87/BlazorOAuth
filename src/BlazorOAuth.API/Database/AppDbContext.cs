namespace BlazorOAuth.API.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
