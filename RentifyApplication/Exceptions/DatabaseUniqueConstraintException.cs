namespace RentifyApplication.Exceptions;

public sealed class DatabaseUniqueConstraintException : Exception
{
    public DatabaseUniqueConstraintException(string constraintName, Exception innerException) : base("A database unique constraint was violated.", innerException)
    {
        ConstraintName = constraintName;
    }

    public string ConstraintName { get; }
}