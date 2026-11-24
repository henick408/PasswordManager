namespace PasswordManager.Utils;

/// <summary>Strength of a password, from the entropy estimate of <see cref="PasswordStrength.Evaluate"/>.</summary>
public enum PasswordStrengthLevel
{
    /// <summary>Empty password.</summary>
    None,
    /// <summary>Below 28 bits.</summary>
    VeryWeak,
    /// <summary>28–35 bits.</summary>
    Weak,
    /// <summary>36–59 bits.</summary>
    Fair,
    /// <summary>60–79 bits.</summary>
    Strong,
    /// <summary>80 bits and more.</summary>
    VeryStrong
}

/// <summary>
/// Estimates password strength as entropy:
/// <code>
/// N     = sum of the character pools used: lowercase 26, uppercase 26, digits 10,
///         ASCII symbols 33, other characters (e.g. "ą", emoji) 100
/// L_eff = effective length: a character is not counted when it is equal or adjacent
///         (code point ± 1) to the previous one, so "aaaa", "abcd", "4321" count as 1
/// E     = L_eff · log2(N) bits
/// </code>
/// It does not know dictionaries, so passwords made of common words are overestimated.
/// </summary>
public static class PasswordStrength
{
    private const int LowercasePool = 26;
    private const int UppercasePool = 26;
    private const int DigitPool = 10;
    private const int AsciiSymbolPool = 33;
    private const int OtherPool = 100;

    /// <summary>Computes the entropy estimate and the matching level.</summary>
    /// <param name="password">Password to evaluate; <c>null</c> or empty gives <see cref="PasswordStrengthLevel.None"/>.</param>
    /// <returns>Entropy in bits and its level.</returns>
    public static (double Bits, PasswordStrengthLevel Level) Evaluate(string? password)
    {
        if (string.IsNullOrEmpty(password)) return (0, PasswordStrengthLevel.None);

        bool hasLower = false, hasUpper = false, hasDigit = false, hasSymbol = false, hasOther = false;
        int effectiveLength = 0;
        char? previous = null;

        foreach (char character in password)
        {
            if (character is >= 'a' and <= 'z') hasLower = true;
            else if (character is >= 'A' and <= 'Z') hasUpper = true;
            else if (character is >= '0' and <= '9') hasDigit = true;
            else if (character < 128) hasSymbol = true;
            else hasOther = true;

            // Repeated characters and simple sequences add (almost) no guessing work.
            if (previous is null || Math.Abs(character - previous.Value) > 1)
            {
                effectiveLength++;
            }
            previous = character;
        }

        int poolSize = (hasLower ? LowercasePool : 0)
                       + (hasUpper ? UppercasePool : 0)
                       + (hasDigit ? DigitPool : 0)
                       + (hasSymbol ? AsciiSymbolPool : 0)
                       + (hasOther ? OtherPool : 0);

        double bits = effectiveLength * Math.Log2(poolSize);
        return (bits, ToLevel(bits));
    }

    /// <summary>Maps entropy to a level (thresholds 28 / 36 / 60 / 80 bits).</summary>
    private static PasswordStrengthLevel ToLevel(double bits) => bits switch
    {
        < 28 => PasswordStrengthLevel.VeryWeak,
        < 36 => PasswordStrengthLevel.Weak,
        < 60 => PasswordStrengthLevel.Fair,
        < 80 => PasswordStrengthLevel.Strong,
        _ => PasswordStrengthLevel.VeryStrong
    };
}
