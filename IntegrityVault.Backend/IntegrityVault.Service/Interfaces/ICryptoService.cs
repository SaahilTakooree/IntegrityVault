// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the ICryptoService service interface, which will be implemented by the crypto service.
    public interface ICryptoService
    {
        byte[] Encrypt(string plainText); // Method to encrypt data.
        string Decrypt(byte[] cipherData); // Method to decrypt data.
    }
}
