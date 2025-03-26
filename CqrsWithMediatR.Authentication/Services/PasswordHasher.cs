using System;
using System.Security.Cryptography;

namespace CqrsWithMediatR.Authentication.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        /*
            Max Non-Hashed Password Length (Maximum length for clear-text passwords:
            ==============================
            The maximum length of a user's password should balance security and usability.

            Recommendation: 64 Characters
            -----------------------------

            Why 64?
            -------
            It's long enough to allow users to create secure and memorable passphrases but avoids excessively long passwords that could be abused (e.g., DoS attacks by hashing huge passwords).
            Most modern systems set limits between 32 and 128 characters for user passwords.
            
            Considerations:
            ---------------
            * Minimum Length: Set a reasonable minimum password length (e.g., 8 or 12 characters) to encourage strong passwords.
            * Encourage Strong Passwords: Instead of only focusing on length, guide users to create secure passwords using uppercase, lowercase, numbers, and special characters.
            * Truncate Excessive Lengths: To prevent abuse, truncate or reject passwords exceeding the maximum length before hashing.

            Recommendation:
            ---------------
            Maximum length for clear-text passwords should be 50 charactes

            Database Column Length for Hashed Passwords
            ===========================================
            Factors Affecting Length:

            Salt Length:
            ------------
            If the salt is 16 bytes (128 bits), Base64 encoding will result in a string of ~24 characters.

            Hash Length:
            -----------
            For a 32-byte (256-bit) hash, Base64 encoding results in ~44 characters.

            Separator:
            ----------
            A delimiter (e.g., a period .) separates the salt and hash.

            Iterations (if stored, but typically not included):
            ---------------------------------------------------
            If stored, this may add a few extra bytes. For simplicity, it’s often omitted as it’s usually static.

            Estimated Length
            ----------------
            * Salt (16 bytes): 24 characters (Base64)
            * Hash (32 bytes): 44 characters (Base64)
            * Separator: 1 character
            * Total: 24 + 44 + 1 = 69 characters
             
            Recommendation: 100 Characters
            ------------------------------
            Use VARCHAR(100) for the hashed password column to allow flexibility for future adjustments (e.g., increased hash size, longer salts, or additional metadata).
            
            This size is sufficient for most modern hashing algorithms like bcrypt, PBKDF2, or Argon2.
        */

        private const int SaltSize = 16;         // 128-bit salt
        private const int KeySize = 32;          // 256-bit hash
        private const int Iterations = 150000;

        private readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("An invalid string was passed to this method.");
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithm,
                    outputLength: KeySize);

                return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("An invalid string was passed to this method.");
            }
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new ArgumentException("An invalid string was passed to this method.");
            }

            var parts = hashedPassword.Split('.', 2);
            if (parts.Length != 2)
            {
                throw new FormatException("Invalid hash format.");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithm,
                outputLength: KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
        }
    }
}
