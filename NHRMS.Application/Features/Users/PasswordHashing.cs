using System.Security.Cryptography;

namespace NHRMS.Application.Features.Users;

public static class PasswordHashing
{
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "PBKDF2-SHA256";

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var derived = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(derived)}";
    }

    public static bool Verify(string password, string storedHash, out bool needsUpgrade)
    {
        needsUpgrade = false;
        var parts = storedHash.Split('$');
        if (parts.Length == 4 && parts[0] == Prefix
            && int.TryParse(parts[1], out var iterations)
            && iterations is >= 100_000 and <= 2_000_000)
        {
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);
                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                needsUpgrade = iterations < Iterations || salt.Length < SaltSize || expected.Length < HashSize;
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Existing accounts were stored as unsalted SHA-256. Successful login upgrades them.
        try
        {
            var expected = Convert.FromHexString(storedHash);
            var actual = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password));
            if (expected.Length != actual.Length)
                return false;
            var valid = CryptographicOperations.FixedTimeEquals(actual, expected);
            needsUpgrade = valid;
            return valid;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
