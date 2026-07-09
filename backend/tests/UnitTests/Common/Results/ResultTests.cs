using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.UnitTests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithoutError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        var error = new Error("sample.error", "A sample failure.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_ShouldThrow_WhenErrorIsNone()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void GenericSuccess_ShouldExposeValue()
    {
        var result = Result<string>.Success("ready");

        Assert.True(result.IsSuccess);
        Assert.Equal("ready", result.Value);
    }

    [Fact]
    public void GenericFailure_ShouldThrow_WhenAccessingValue()
    {
        var result = Result<string>.Failure(new Error("sample.error", "A sample failure."));

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }
}
