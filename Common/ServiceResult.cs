

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static implicit operator ServiceResult<T>(FailureResult failureResult)
    {
        return new()
        {
            IsSuccess = false,
            StatusCode = failureResult.StatusCode,
            ErrorMessage = failureResult.ErrorMessage,
            Data = default
        };
    }
}

public class FailureResult
{
    public required int StatusCode { get; set; }
    public required string ErrorMessage { get; set; }
}


public class ServiceResult
{
    public required int StatusCode { get; set; }
    public List<string>? Warnings { get; set; }
    public string? ErrorMessage { get; set; }
    public required bool IsSuccess { get; set; }

    public static implicit operator ServiceResult(FailureResult failureResult) => new()
    {
        IsSuccess = false,
        StatusCode = failureResult.StatusCode,
        ErrorMessage = failureResult.ErrorMessage,
    };

    // Success result without warings
    public static ServiceResult<T> Success<T>(int statusCode, T data) => new()
    {
        StatusCode = statusCode,
        Data = data,
        IsSuccess = true,
    };
    // Success result with warnings
    public static ServiceResult<T> Success<T>(int statusCode, T data, List<string>? warnings) => new()
    {
        StatusCode = statusCode,
        Data = data,
        IsSuccess = true,
        Warnings = warnings,
    };
    public static ServiceResult Success(int statusCode) => new()
    {
        StatusCode = statusCode,
        IsSuccess = true
    };
    public static FailureResult Failure(int statusCode, string errorMessage) => new()
    {
        StatusCode = statusCode,
        ErrorMessage = errorMessage,
    };





    // Common Results
    public static ServiceResult<T> Ok<T>(T Data = default) => Success(StatusCodes.Status200OK, Data);
    public static ServiceResult<T> Ok<T>(T Data, List<string>? warnings) => Success(StatusCodes.Status200OK, Data, warnings);
    public static ServiceResult NoContent() => Success(StatusCodes.Status204NoContent);
    public static FailureResult Unauthorized(string errorMessage = "401 Unauthorized") => Failure(StatusCodes.Status401Unauthorized, errorMessage);
    public static FailureResult Forbidden(string errorMessage = "403 Forbidden") => Failure(StatusCodes.Status403Forbidden, errorMessage);
    public static FailureResult BadRequest(string errorMessage = "400 Bad Request") => Failure(StatusCodes.Status400BadRequest, errorMessage);
    public static FailureResult NotFound(string errorMessage = "404 Not Found") => Failure(StatusCodes.Status404NotFound, errorMessage);
    public static FailureResult Conflict(string errorMessage = "409 Connflict") => Failure(StatusCodes.Status409Conflict, errorMessage);
    public static FailureResult InternalServerError(string errorMessage = "Internal Server Error") => Failure(StatusCodes.Status500InternalServerError, errorMessage);
}