using System.Text;

namespace Tracking.Security.Password;

public sealed class PasswordValidator
{
    private static readonly HashSet<string> CommonPasswords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "password123",
            "p@ssw0rd",
            "p@ssword",
            "passw0rd",
            "secret",
            "secret123",
            "admin",
            "admin123",
            "administrator",
            "administrator123",
            "qwerty",
            "qwerty123",
            "welcome",
            "welcome123",
            "letmein",
            "changeme",
            "123456",
            "12345678",
            "123456789",
            "1234567890",
            "111111",
            "000000"
        };

    private readonly PasswordPolicy _policy;

    public PasswordValidator(PasswordPolicy? policy = null)
    {
        _policy = policy ?? new PasswordPolicy();
    }

    public PasswordValidationResult Validate(
        string password,
        string? username = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required.");
            return PasswordValidationResult.Failure(errors);
        }

        if (password.Length < _policy.MinimumLength)
        {
            errors.Add(
                $"Password must contain at least {_policy.MinimumLength} characters.");
        }

        if (_policy.RequireUppercase &&
            !password.Any(char.IsUpper))
        {
            errors.Add(
                "Password must contain at least one uppercase character.");
        }

        if (_policy.RequireLowercase &&
            !password.Any(char.IsLower))
        {
            errors.Add(
                "Password must contain at least one lowercase character.");
        }

        if (_policy.RequireDigit &&
            !password.Any(char.IsDigit))
        {
            errors.Add(
                "Password must contain at least one digit.");
        }

        if (_policy.RequireSpecialCharacter &&
            !password.Any(IsSpecialCharacter))
        {
            errors.Add(
                "Password must contain at least one special character.");
        }

        if (HasRepeatedCharacters(password))
        {
            errors.Add(
                "Password must not contain more than two identical characters in a row.");
        }

        if (HasSequentialCharacters(password))
        {
            errors.Add(
                "Password must not contain sequential characters.");
        }

        if (_policy.RejectUsername &&
            !string.IsNullOrWhiteSpace(username) &&
            ContainsUsername(password, username))
        {
            errors.Add(
                "Password must not contain the username.");
        }

        if (_policy.RejectCommonPasswords &&
            IsCommonPassword(password))
        {
            errors.Add(
                "Password is too common or dictionary-based.");
        }

        return errors.Count == 0
            ? PasswordValidationResult.Success()
            : PasswordValidationResult.Failure(errors);
    }

    private bool HasRepeatedCharacters(string value)
    {
        var count = 1;

        for (var i = 1; i < value.Length; i++)
        {
            if (char.ToLowerInvariant(value[i]) ==
                char.ToLowerInvariant(value[i - 1]))
            {
                count++;

                if (count > _policy.MaximumIdenticalCharactersInRow)
                    return true;
            }
            else
            {
                count = 1;
            }
        }

        return false;
    }

    private bool HasSequentialCharacters(string value)
    {
        if (value.Length < _policy.MaximumSequentialCharacters + 1)
            return false;

        var normalized = value.ToLowerInvariant();

        for (var i = 0; i <= normalized.Length - 3; i++)
        {
            var a = normalized[i];
            var b = normalized[i + 1];
            var c = normalized[i + 2];

            if (!char.IsLetterOrDigit(a) ||
                !char.IsLetterOrDigit(b) ||
                !char.IsLetterOrDigit(c))
            {
                continue;
            }

            if (b == a + 1 && c == b + 1)
                return true;

            if (b == a - 1 && c == b - 1)
                return true;
        }

        return false;
    }

    private static bool ContainsUsername(
        string password,
        string username)
    {
        return password.Contains(
            username,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCommonPassword(string password)
    {
        if (CommonPasswords.Contains(password))
            return true;

        var candidates = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            password
        };

        // Treat common punctuation added around a dictionary word
        // as decoration, e.g. "password!" or "!password".
        var trimmed = password.Trim(
            '!', '@', '#', '$', '%', '^', '&', '*',
            '(', ')', '-', '_', '+', '=', '.', ',', '?');

        if (!string.Equals(
            trimmed,
            password,
            StringComparison.Ordinal))
        {
            candidates.Add(trimmed);
        }

        foreach (var candidate in candidates)
        {
            if (CommonPasswords.Contains(candidate))
                return true;

            var normalized = NormalizeLeet(candidate);

            if (CommonPasswords.Contains(normalized))
                return true;

            var compact = new string(
                normalized
                    .Where(char.IsLetterOrDigit)
                    .ToArray());

            if (CommonPasswords.Contains(compact))
                return true;

            foreach (var commonPassword in CommonPasswords)
            {
                var commonNormalized = NormalizeLeet(commonPassword);

                var commonCompact = new string(
                    commonNormalized
                        .Where(char.IsLetterOrDigit)
                        .ToArray());

                if (string.Equals(
                    compact,
                    commonCompact,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string NormalizeLeet(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.ToLowerInvariant())
        {
            builder.Append(character switch
            {
                '@' => 'a',
                '4' => 'a',
                '3' => 'e',
                '1' => 'i',
                '!' => 'i',
                '0' => 'o',
                '$' => 's',
                '5' => 's',
                '7' => 't',
                _ => character
            });
        }

        return builder.ToString();
    }

    private static bool IsSpecialCharacter(char character)
    {
        return !char.IsLetterOrDigit(character) &&
               !char.IsWhiteSpace(character);
    }
}
