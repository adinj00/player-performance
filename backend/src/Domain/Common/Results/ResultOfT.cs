using PlayerPerformance.Domain.Common.Errors;

namespace PlayerPerformance.Domain.Common.Results;

public sealed class Result<TValue> : Result
{
    private readonly TValue? value;

    private Result(TValue value)
        : base(true, Error.None)
    {
        this.value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
        value = default;
    }

    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<TValue> Success(TValue value)
    {
        return new(value);
    }

    public static new Result<TValue> Failure(Error error)
    {
        return new(error);
    }
}
