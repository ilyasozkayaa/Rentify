using FluentValidation.TestHelper;
using RentifyApplication.Command.CreateImageUploadBatch;
using RentifyApplication.Constants;
using System.Reflection.Metadata;

namespace RentifyUnitTests.ValidatorTests;

public sealed class CreateImageUploadBatchCommandValidatorTests
{
    private readonly CreateImageUploadBatchCommandValidator _validator = new();

    [Fact]
    public void Should_accept_supported_images_within_count_and_size_limits()
    {
        var command = new CreateImageUploadBatchCommand(12,
        [
            new ImageUploadFile("image/jpeg", 1024),
            new ImageUploadFile("image/png", ProjectConstants.MaximumFileSize),
            new ImageUploadFile("image/webp", 2048)
        ]);

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_unsupported_type_oversized_file_and_too_many_images()
    {
        var files = Enumerable.Repeat(new ImageUploadFile("image/gif", ProjectConstants.MaximumFileSize + 1), 9).ToArray();

        var result = _validator.TestValidate(new CreateImageUploadBatchCommand(12, files));

        result.ShouldHaveValidationErrorFor("Files");
        result.ShouldHaveValidationErrorFor("Files[0].ContentType");
        result.ShouldHaveValidationErrorFor("Files[0].FileSize");
    }
}
