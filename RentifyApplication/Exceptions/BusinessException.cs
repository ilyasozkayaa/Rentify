using RentifyApplication.Exceptions.Enums;

namespace RentifyApplication.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message, BusinessErrorCode code, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public BusinessErrorCode Code { get; }
    public int StatusCode { get; }
}
