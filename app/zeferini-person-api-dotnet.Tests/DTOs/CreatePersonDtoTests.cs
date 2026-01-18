using Xunit;
using ZeferiniPersonApi.DTOs;
using FluentValidation;

public class CreatePersonDtoTests
{
    [Fact]
    public void ValidateAndThrow_ValidDto_DoesNotThrow()
    {
        var dto = new CreatePersonDto { Name = "Ada Lovelace", Email = "ada@example.com" };
        dto.ValidateAndThrow(); // Should not throw
    }

    [Fact]
    public void ValidateAndThrow_InvalidName_ThrowsValidationException()
    {
        var dto = new CreatePersonDto { Name = "", Email = "ada@example.com" };
        Assert.Throws<ValidationException>(() => dto.ValidateAndThrow());
    }

    [Fact]
    public void ValidateAndThrow_InvalidEmail_ThrowsValidationException()
    {
        var dto = new CreatePersonDto { Name = "Ada Lovelace", Email = "not-an-email" };
        Assert.Throws<ValidationException>(() => dto.ValidateAndThrow());
    }
}