// Import dependencies.
using IntegrityVault.Common.DTOs; // Data Transfer Objects for communication.
using IntegrityVault.Common.Entities; // Entity models representing the domain objects.


// Declaring the namespace where the mappers resides.
namespace IntegrityVault.Service.Mappers
{
    // Mapper class to convert data to a MedicalRecord entity.
    internal static class ToMedicalRecordMapper
    {
        // Method to map the relevant fields to a MedicalRecord entity.
        internal static MedicalRecord ToMedicalRecordEntity (int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO, string IPFS_CID, 
            string contentHashHex, string versionHashHex, string? blockchainTxHashint, int version, DateTime currentTime)
        {
            // Return a new MedicalRecord entity, with all necessary data populated.
            return new MedicalRecord
            {
                EpisodeID = episodeID,
                VisitDate = createMedicalRecordDTO.VisitDate,
                IPFS_CID = IPFS_CID,
                CurrentVersion = version,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
                ContentHash = contentHashHex,
                VersionHash = versionHashHex,
                BlockchainTxHash = blockchainTxHashint
            };
        }
    }
}
