using FluentValidation;

namespace RentifyApplication.Command.CreateImageUploadBatch;

public sealed class CreateImageUploadBatchCommandValidator : AbstractValidator<CreateImageUploadBatchCommand>
{
    public const int MaximumImages = 3;
    public const long MaximumFileSize = 5 * 1024 * 1024;
    public static readonly string[] SupportedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public CreateImageUploadBatchCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).GreaterThan(0);
        RuleFor(x => x.Files).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Must(files => files.Count <= MaximumImages)
            .WithMessage($"A maximum of {MaximumImages} images can be uploaded.");
        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(x => x.ContentType).Must(type => SupportedContentTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Only JPEG, PNG and WebP images are supported.");
            file.RuleFor(x => x.FileSize).GreaterThan(0).LessThanOrEqualTo(MaximumFileSize)
                .WithMessage("Image files must be no larger than 5 MB.");
        });
    }
}
