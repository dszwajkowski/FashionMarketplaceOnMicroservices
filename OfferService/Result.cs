namespace OfferService;

public class Result<T>
{
    public T? Data { get; private set; }
    public ErrorType? ErrorType { get; private set; }
    public IEnumerable<string>? ErrorMessages { get; private set; }
    public bool IsSuccess => ErrorType is null;

    public Result(T? data)
    {
        Data = data;
    }

    public Result(ErrorType errorType, IEnumerable<string> errorMessages)
    {
        ErrorType = errorType;
        ErrorMessages = errorMessages;
    }


    public Result(ErrorType errorType, string errorMessage)
    {
        ErrorType = errorType;
        ErrorMessages = new List<string> { errorMessage };
    }

    //public static implicit operator Result<T>(T? value) => new Result<T>(value);
}

public enum ErrorType
{
    NotFound,
    Validation,
    Forbidden,
    Unauthorized,
    Invalid
}
