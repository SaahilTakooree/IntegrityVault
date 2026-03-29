// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO for create medical record audit log.
    public class CreateMedicalRecordAuditDTO
    {
        public int RecordID { get; set; }
        public int UpdatedByDoctorID { get; set; }
        public required string PreviousIPFS_CID {  get; set; }
        public required string NewIPFS_CID { get; set; }
        public required string PreviousContentHash { get; set; }
        public required string NewContentHash { get; set; }
        public required string PreviousVersionHash { get; set; }
        public required string NewVersionHash { get; set; }
        public required string BlockchainTxHash { get; set; }
        public int Version { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
