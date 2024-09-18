namespace BlazorOAuth.API.Utils;

public sealed class Result
{
    public ResultType Type { get; init; } = ResultType.Undefined;
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; init; }

    internal Result(bool isSuccess, string message, ResultType type)
    {
        IsSuccess = isSuccess;
        Message = message;
        Type = type;
    }

    public static Result<T> Success<T>(T payload)
        => new(isSuccess: true, payload, message: null!, ResultType.Success);
    public static Result Success()
        => new(isSuccess: true, message: null!, ResultType.Success);

    public static Result<T> Failure<T>(ResultType type = ResultType.Failure, string? errorMessage = "")
        => new(isSuccess: false, default!, message: errorMessage!, type);

    public static Result Failure(ResultType type = ResultType.Failure, string? errorMessage = "")
        => new(isSuccess: false, message: errorMessage!, type);
}

public sealed class Result<T>
{
    private readonly T _payload;

    public T Payload
    {
        get => IsSuccess ? _payload : throw new InvalidOperationException("Cannot access payload of a failed result.");
    }

    public ResultType Type { get; init; } = ResultType.Undefined;
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; init; }

    internal Result(bool isSuccess, T payload, string message, ResultType type)
    {
        _payload = payload;
        IsSuccess = isSuccess;
        Message = message;
        Type = type;
    }
}

public enum ResultType
{
    Undefined = 0,
    Success = 1,
    Failure = 2,
    NotFound = 3,
    NotAllowed = 4
}
