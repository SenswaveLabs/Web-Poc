namespace Senswave.Web.Shared.Resulting;

public class Result
{
    public Error[] Errors { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error[] errors)
    {
        foreach (var error in errors)
        {
            if (isSuccess && error != Error.None)
                throw new ArgumentException("Success result cannot have an error.", nameof(isSuccess));

            if (!isSuccess && error == Error.None)
                throw new ArgumentException("Failure result must have an error.", nameof(errors));
        }
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, [Error.None]);
    public static Result Failure(Error[] errors) => new(false, errors);
    public static Result Failure(Error error, Error[] secondaryErrors) => new(false, [error, .. secondaryErrors]);
    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure() => new(false, [Error.Unknown]);

    public static bool operator true(Result result) => result.IsSuccess;
    public static bool operator false(Result result) => result.IsFailure;

    public static bool operator !(Result result) => !result.IsSuccess;
}

public class Result<T> : Result
{
    public T Value { get; }

    protected Result(bool isSuccess, Error[] errors, T value = default!) : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, [Error.None], value);
    public new static Result<T> Failure(Error[] errors) => new(false, errors);
    public new static Result<T> Failure(Error error, Error[] secondaryErrors) => new(false, [error, .. secondaryErrors]);
    public new static Result<T> Failure(Error error) => new(false, [error]);
    public new static Result<T> Failure() => new(false, [Error.Unknown]);

    public static bool operator true(Result<T> result) => result.IsSuccess;
    public static bool operator false(Result<T> result) => result.IsFailure;
}

