namespace BlazorOAuth.API.Services;

public interface IUserService
{
    public Task<Result<User>> CreateAsync(CreateUserCommand userDto, CancellationToken cancellationToken = default);
    public Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default);
    public Task<Result> DeleteAsync(int userId, CancellationToken cancellationToken = default);
    public Task<Result<User>> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    public Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    public Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken);
}

internal class UserService(AppDbContext dbContext) : IUserService
{
    public async Task<Result<User>> CreateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            ExternalId = request.ExternalId,
            Username = request.Username,
            Email = request.Email,
            ProfilePicture = request.ProfilePicture,
            SignInCount = 1
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(user);
    }

    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(user);
    }

    public async Task<Result> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is not null) dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        return user is not null
            ? Result.Success(user)
            : Result.Failure<User>(ResultType.NotFound, "User not found");
    }

    public async Task<Result<User>> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);

        return user is not null
            ? Result.Success(user)
            : Result.Failure<User>(ResultType.NotFound, "User not found");
    }

    public async Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is not null
            ? Result.Success(user)
            : Result.Failure<User>(ResultType.NotFound, "User not found");
    }

    
}
