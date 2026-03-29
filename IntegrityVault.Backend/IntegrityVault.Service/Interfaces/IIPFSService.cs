// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IIPFSService service interface, which will be implemented by the IPFS service.
    public interface IIPFSService
    {
        Task<string> AddFileAsync(byte[] fileBytes); // Method to encrypts and upload a file to IPFS. Return the CID.
        Task<byte[]> GetFileAsync(string CID); // Method to retrieve and decrypts a file from IPFS by CID.
        Task<string> GetCIDOnlyAsync(byte[] fileBytes); // Method to computes the CID of bytes without uploading the IPFS.
    }
}
