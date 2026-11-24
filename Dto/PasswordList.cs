namespace PasswordManager.Dto;

/// <summary>Result of loading the vault.</summary>
/// <param name="Entries">Entries that were decrypted and authenticated successfully.</param>
/// <param name="SkippedCount">
/// Number of rows that could not be decrypted (tampered, moved between rows/users,
/// corrupted or in an unsupported version) and were left out of <paramref name="Entries"/>.
/// </param>
public sealed record PasswordList(IList<PasswordEntry> Entries, int SkippedCount);
