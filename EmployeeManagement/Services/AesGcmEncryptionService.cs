
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagement.Services
{
    public sealed record EncryptedValue(
        byte[] Ciphertext,
        byte[] Nonce,
        byte[] Tag,
        int KeyVersion
    );

    public class AesGcmEncryptionService
    {
        private readonly byte[] _key;
        private readonly int _keyVersion;

        public AesGcmEncryptionService(IConfiguration configuration)
        {
            // Read the AES key from User Secrets
            string? base64Key = configuration["Encryption:AesKey"];

            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new InvalidOperationException(
                    "AES encryption key is missing from configuration.");
            }

            try
            {
                _key = Convert.FromBase64String(base64Key);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "AES encryption key must be valid Base64.", ex);
            }

            // AES-256 requires exactly 32 bytes
            if (_key.Length != 32)
            {
                throw new InvalidOperationException(
                    "AES-256 requires a 32-byte key.");
            }

            // Read the encryption key version
            string? version = configuration["Encryption:KeyVersion"];

            if (!int.TryParse(version, out _keyVersion) ||
                _keyVersion <= 0)
            {
                throw new InvalidOperationException(
                    "A valid positive encryption key version is required.");
            }
        }

        public EncryptedValue Encrypt(
            string plaintext,
            byte[]? associatedData = null)
        {
            ArgumentNullException.ThrowIfNull(plaintext);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Generate a fresh 12-byte nonce for every encryption
            byte[] nonce = RandomNumberGenerator.GetBytes(12);

            // Allocate space for encrypted data and authentication tag
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(_key, 16))
            {
                aes.Encrypt(
                    nonce,
                    plaintextBytes,
                    ciphertext,
                    tag,
                    associatedData);
            }

            return new EncryptedValue(
                ciphertext,
                nonce,
                tag,
                _keyVersion);
        }

        public string Decrypt(
            byte[] ciphertext,
            byte[] nonce,
            byte[] tag,
            byte[]? associatedData = null)
        {
            ArgumentNullException.ThrowIfNull(ciphertext);
            ArgumentNullException.ThrowIfNull(nonce);
            ArgumentNullException.ThrowIfNull(tag);

            if (nonce.Length != 12 || tag.Length != 16)
            {
                throw new CryptographicException(
                    "Invalid nonce or authentication tag.");
            }

            byte[] plaintextBytes = new byte[ciphertext.Length];

            using (var aes = new AesGcm(_key, 16))
            {
                aes.Decrypt(
                    nonce,
                    ciphertext,
                    tag,
                    plaintextBytes,
                    associatedData);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
