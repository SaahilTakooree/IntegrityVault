// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for medical record audit log data, such as CreateMedicalRecordAuditDTO.
using IntegrityVault.Common.Entities; // Imported to allow access to the MedicalRecord entity.


// Declare the namespace for the repository interfaces..
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IMedicalRecordAuditLogRepository interface that represents the contract for medical record audit logs related database operations.
    public interface IMedicalRecordAuditLogRepository
    {
        Task<List<MedicalRecordAuditLog>> GetAllVersionOfMedicalRecordByID(int medicalID); // Method to get the version of medical record.
        Task<bool> InsertAuditLog(CreateMedicalRecordAuditDTO createMedicalRecordAuditDTO); // Method to insert a new audit log and return true if successful.
        Task<MedicalRecordAuditLog?> GetAuditLogByNewCIDAsync(string cid); // Method to returns the audit log for a CID that points to a newer version of a medical record.
        Task<MedicalRecordAuditLog?> GetAuditLogByPreviousCIDAsync(string cid); // Returns the audit log for a CID that points to an older version of a medical record.
    }
}
