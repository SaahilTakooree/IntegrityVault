// Declare the namespace for the service interfaces.
using IntegrityVault.Common.DTOs;

namespace IntegrityVault.Service.Interfaces
{
    // Define the IBlockchainService interface, which will be implemented by the blockchain service.
    public interface IBlockchainService
    {
        Task AddHospitalToChainAsync(int hospitalID, string walletAddress); // Method to add the hospital as an authorise user on the smart contract.
        Task UpdateHospitalWalletOnChainAsync(int hospitalID, string walletAddress); // Update the detail of an authorised user on the smart contract.
        Task DeleteHospitalWalletFromChainAsync(int hospitalID); // Method to remove a hospital as an authorise user on the smart contract.
        Task<string> RegisterRecordOnChainAsync(int hospitalId, int recordId, int episodeId, byte[] contentHash, byte[] versionHash, string ipfsCid); // Method to add medical record on the chain.
        Task<string> UpdateRecordOnChainAsync(int hospitalId, int recordId, int currentVersion, byte[] newContentHash, byte[] newVersionHash, string newIpfsCid); // Method to update the medical record on the chain.
        Task<RecordEntryOutput> GetRecordFromChainAsync(int recordId, int version); // Method to a record from the chain.
        Task<RecordEntryOutput> GetLatestRecordFromChainAsync(int recordId); // Method to get the latest record from the chain.
    }
}
