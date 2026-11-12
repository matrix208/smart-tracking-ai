namespace Tracking.Security.Password;

public sealed class PasswordValidationResult
{
    private PasswordValidationResult(
        bool isValid,
        IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static PasswordValidationResult Success()
        => new(true, Array.Empty<string>());

    public static PasswordValidationResult Failure(
        IEnumerable<string> errors)
        => new(false, errors.ToArray());
}
