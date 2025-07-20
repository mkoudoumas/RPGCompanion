using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace RPGCompanion.Server.Services;

public static class PasswordService
{
    private const int SaltSize = 16;   // bytes
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 4,
            MemorySize = 1 << 16  // 64 MB
        };
        var hash = argon2.GetBytes(HashSize);
        return $"{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('|');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 4,
            MemorySize = 1 << 16
        };
        var hash = argon2.GetBytes(HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }
}
