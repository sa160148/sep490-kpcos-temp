using KPCOS.BusinessLayer.Exceptions;

namespace KPCOS.BusinessLayer.DTOs;

public class CustomResult<T>
{
    private readonly T? _value;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public CustomError? CustomError { get; }

    public static CustomResult<T> Success(T value) => new CustomResult<T>(value);
    public static CustomResult<T> Failure(CustomError error) => new CustomResult<T>(error);

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("There is no value for failure result.");
            return _value!;
        }
        private init => _value = value;
    }

    private CustomResult(T value)
    {
        Value = value;
        IsSuccess = true;
        CustomError = CustomError.None;
    }

    private CustomResult(CustomError customError)
    {
        if (customError == CustomError.None)
            throw new ArgumentException("CustomError cannot be None", nameof(customError));
        IsSuccess = false;
        CustomError = customError;
    }
    public CustomResult(bool isSuccess, CustomError customError)
    {
        if (isSuccess && customError != CustomError.None ||
            !isSuccess && customError == CustomError.None)
        {
            throw new ArgumentException("Invalid customError", nameof(customError));
        }

        IsSuccess = isSuccess;
        CustomError = customError;
    }
}