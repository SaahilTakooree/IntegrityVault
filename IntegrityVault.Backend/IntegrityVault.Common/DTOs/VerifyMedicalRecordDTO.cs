// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO use when verifing medical record endpoints.
    public class VerifyMedicalRecordDTO
    {
        public bool IsTampered { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool ContentHashMatch { get; set; }
        public bool DatabaseHashMatch { get; set; }
        public bool CIDMatch { get; set; }
        public bool VersionHashMatch { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}