using Xunit;
using Tracking.Security.Password;

namespace Tracking.Security.Tests;

public sealed class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [Theory]
    [InlineData("Az7!qW9@Lm")]
    [InlineData("Xy9#Km2@q")]
    [InlineData("Secure7!Pass")]
    public void Strong_password_is_accepted(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.True(result.IsValid, string.Join(" | ", result.Errors));
    }

    [Theory]
    [InlineData("abcdef12!")]
    [InlineData("ABCDEF12!")]
    [InlineData("Abcdefgh!")]
    [InlineData("Abcdef12")]
    public void Missing_required_character_class_is_rejected(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Abc111!x")]
    [InlineData("Aaa123!x")]
    [InlineData("111Abc!x")]
    public void Repeated_characters_are_rejected(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Abc123!x")]
    [InlineData("Abc789!x")]
    [InlineData("Abc456!x")]
    [InlineData("Xyz!123Ab")]
    public void Sequential_characters_are_rejected(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Admin123!")]
    [InlineData("admin123!")]
    [InlineData("MyAdmin!7")]
    public void Username_is_rejected(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("password123!")]
    [InlineData("P@ssw0rd!")]
    [InlineData("Secret123!")]
    public void Common_dictionary_passwords_are_rejected(string password)
    {
        var result = _validator.Validate(password, "admin");

        Assert.False(result.IsValid);
    }
}
