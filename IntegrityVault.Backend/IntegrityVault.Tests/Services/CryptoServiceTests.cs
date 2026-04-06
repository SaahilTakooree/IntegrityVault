using System.Security.Cryptography;
using System.Text;

namespace IntegrityVault.Tests.Services;


// Define the test suite for the CryptoService implementation.
public class CryptoServiceTests
{
    private readonly byte[] _validKey;
    private readonly CryptoService _service;


    public CryptoServiceTests()
    {
        // Initialise a valid 32-byte (256-bit) key for AES.
        _validKey = new byte[32];
        RandomNumberGenerator.Fill(_validKey);

        // Initialise the service with the key.
        _service = new CryptoService(_validKey);
    }



    #region Encryption and Decryption Happy Paths

    [Theory]
    [InlineData("Hello Integrity Vault!")]
    [InlineData("VerySecretPrivateKey123")]
    [InlineData("1234567890")]
    [InlineData(" ")]
    public void EncryptAndDecrypt_ShouldReturnOriginalString_WhenKeyIsValid(string plainText)
    {
        // Act: Encrypt the plain text.
        byte[] encryptedData = _service.Encrypt(plainText);

        // Act: Decrypt the resulting cipher data.
        string decryptedText = _service.Decrypt(encryptedData);

        // Assert: Verify the round-trip result matches the input.
        Assert.Equal(plainText, decryptedText);
        Assert.NotEqual(plainText, Encoding.UTF8.GetString(encryptedData)); // Ensure it's actually encrypted.
    }


    [Fact]
    public void Encrypt_ShouldProduceDifferentResult_ForDifferentInput()
    {
        // Arrange: Prepare two different strings.
        string input1 = "Message One";
        string input2 = "Message Two";

        // Act: Encrypt both.
        byte[] result1 = _service.Encrypt(input1);
        byte[] result2 = _service.Encrypt(input2);

        // Assert: Results should be unique.
        Assert.NotEqual(result1, result2);
    }

    #endregion



    #region Security and Edge Cases

    [Fact]
    public void Decrypt_ShouldThrowException_WhenKeyIsIncorrect()
    {
        // Arrange: Encrypt a message with the correct key.
        string secret = "Secret Message";
        byte[] encryptedData = _service.Encrypt(secret);

        // Create a different 32-byte key.
        byte[] wrongKey = new byte[32];
        RandomNumberGenerator.Fill(wrongKey);
        var wrongService = new CryptoService(wrongKey);

        // Act & Assert: Decrypting with the wrong key should fail authentication (AES-GCM Tag check).
        Assert.Throws<AuthenticationTagMismatchException>(() => wrongService.Decrypt(encryptedData));
    }


    [Fact]
    public void Decrypt_ShouldThrowException_WhenDataIsTampered()
    {
        // Arrange: Encrypt a message.
        byte[] encryptedData = _service.Encrypt("Secure Data");

        // Tamper with the ciphertext part (flip a bit in the last byte).
        encryptedData[^1] ^= 0xFF;

        // Act & Assert: AES-GCM should detect the lack of integrity via the Tag.
        Assert.Throws<AuthenticationTagMismatchException>(() => _service.Decrypt(encryptedData));
    }


    [Fact]
    public void Encrypt_ShouldHandleEmptyString_Successfully()
    {
        // Arrange.
        string empty = string.Empty;

        // Act.
        byte[] encrypted = _service.Encrypt(empty);
        string decrypted = _service.Decrypt(encrypted);

        // Assert: Nonce (12) + Tag (16) + Ciphertext (0) = 28 bytes.
        Assert.Equal(28, encrypted.Length);
        Assert.Equal(empty, decrypted);
    }

    #endregion



    #region Formatting and Structure Tests

    [Fact]
    public void EncryptedArray_ShouldHaveCorrectSize()
    {
        // Arrange.
        string input = "Test"; // 4 bytes in UTF8.

        // Act.
        byte[] result = _service.Encrypt(input);

        // Assert: NonceSize(12) + TagSize(16) + InputSize(4) = 32.
        Assert.Equal(12 + 16 + 4, result.Length);
    }


    [Fact]
    public void Decrypt_ShouldThrowException_WhenDataIsTooShort()
    {
        // Arrange: Create a byte array shorter than Nonce (12) + Tag (16).
        byte[] smallData = new byte[10];

        // Act & Assert: This should throw an ArgumentException or similar when Buffer.BlockCopy runs.
        Assert.ThrowsAny<Exception>(() => _service.Decrypt(smallData));
    }

    #endregion
}