using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using UavFleetManagement.Application.Common.Interfaces;

namespace UavFleetManagement.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public (string Hash, string Salt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8);
        string salt = Convert.ToBase64String(saltBytes);

        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);

        string calculatedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return calculatedHash == hash;
    }
}
