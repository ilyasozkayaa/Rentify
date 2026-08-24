namespace RentifyApplication.Exceptions;

public sealed class ValidationFailedException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(IReadOnlyDictionary<string, string[]> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
