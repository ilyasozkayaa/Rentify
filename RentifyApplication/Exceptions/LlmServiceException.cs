namespace RentifyApplication.Exceptions;

public sealed class LlmServiceException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public LlmServiceException(string code, int statusCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
