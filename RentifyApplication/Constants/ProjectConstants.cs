namespace RentifyApplication.Constants;

public static class ProjectConstants
{
    public const string HeaderName = "Idempotency-Key";
    public const string ProcessingValue = "processing";
    public const string MeterName = "Rentify.LLM";
    public const int MaximumImages = 3;
    public const long MaximumFileSize = 5 * 1024 * 1024;
    public const int UploadExpirySeconds = 900;
    public const int DownloadExpirySeconds = 900;
}