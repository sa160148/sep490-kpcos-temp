namespace KPCOS.BusinessLayer.Exceptions;

public sealed record CustomError(string Code, string? Message = null)
{
    private static readonly string _recordNotFoundCode = "RecordNotFound";
    private static readonly string _validationErrorCode = "ValidationError";
    
    public static readonly CustomError None = new(string.Empty);
    
    public static CustomError RecordNotFound(string? message = null) => new(_recordNotFoundCode, message);
    public static CustomError ValidationError(string? message = null) => new(_validationErrorCode, message);
    
    /*public static implicit operator Result(CustomError customError) => Result.Failure(customError);*/
}