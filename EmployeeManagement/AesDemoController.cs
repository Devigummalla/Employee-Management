using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AesDemoController : Controller
    {
        public IActionResult Demo()
        {
            string originalData =
                "Employee Bank Account: 123456789";

            // Convert text into bytes
            byte[] plaintext =
                Encoding.UTF8.GetBytes(originalData);

            // Generate a 256-bit AES key
            byte[] key =
                RandomNumberGenerator.GetBytes(32);

            // Generate a 12-byte nonce
            byte[] nonce =
                RandomNumberGenerator.GetBytes(12);

            // Create space for ciphertext and authentication tag
            byte[] ciphertext =
                new byte[plaintext.Length];

            byte[] tag = new byte[16];

            // Encrypt using AES-GCM
            using (var aes = new AesGcm(key, tag.Length))
            {
                aes.Encrypt(
                    nonce,
                    plaintext,
                    ciphertext,
                    tag
                );
            }

            // Decrypt using the same key, nonce and tag
            byte[] decryptedData =
                new byte[ciphertext.Length];

            using (var aes = new AesGcm(key, tag.Length))
            {
                aes.Decrypt(
                    nonce,
                    ciphertext,
                    tag,
                    decryptedData
                );
            }

            string decryptedText =
                Encoding.UTF8.GetString(decryptedData);

            return Content(
                $"Original Data: {originalData}\n\n" +
                $"Encrypted Data: {Convert.ToBase64String(ciphertext)}\n\n" +
                $"Decrypted Data: {decryptedText}"
            );
        }
    }
}