namespace Auction.BL.Model.Result;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}