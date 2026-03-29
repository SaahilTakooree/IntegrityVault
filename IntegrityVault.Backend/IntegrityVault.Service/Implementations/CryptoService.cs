// Import dependencies needed.
using IntegrityVault.Service.Interfaces; // Import the interface for the encryption and decryption.
using System.Security.Cryptography; // Provide the cryptography function.
using System.Text; // Provides text encoding functionality.



// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    public class CryptoService(byte[] _key) : ICryptoService
    {
        // AES-GCM standard sizes
        private const int NonceSize = 12;
        private const int TagSize = 16;


        // Method to encrypt plain text.
        public byte[] Encrypt(string plainText)
        {
            // Convert plain text to bytes.
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);

            var nonceSource = SHA256.HashData(plaintextBytes);
            var nonce = new byte[NonceSize];
            Buffer.BlockCopy(nonceSource, 0, nonce, 0, NonceSize);

            // Prepare buffers.
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[TagSize];

            // Perform encryption.
            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // Combine nonce, tag, and ciphertext into a single array.
            var result = new byte[NonceSize + TagSize + ciphertext.Length];

            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

            return result;
        }


        // Method to decrypt cipher text.
        public string Decrypt(byte[] cipherData)
        {
            // Extract nonce.
            var nonce = new byte[NonceSize];
            Buffer.BlockCopy(cipherData, 0, nonce, 0, NonceSize);

            // Extract tag.
            var tag = new byte[TagSize];
            Buffer.BlockCopy(cipherData, NonceSize, tag, 0, TagSize);

            // Extract ciphertext.
            var ciphertextLength = cipherData.Length - NonceSize - TagSize;
            var ciphertext = new byte[ciphertextLength];
            Buffer.BlockCopy(cipherData, NonceSize + TagSize, ciphertext, 0, ciphertextLength);

            // Prepare plaintext buffer.
            var plaintextBytes = new byte[ciphertextLength];

            // Perform decryption.
            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);

            // Convert back to string.
            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
