// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IMedicalRecordAuditLogRespository interface to implement the repository.
using IntegrityVault.Common.Entities; // Import the entity classes representing medical record audit log.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.
using IntegrityVault.Common.DTOs; // Import data transfer objects used in the repository for medical record audit log creation.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IMedicalRecordAuditLogRespository interface, with the DbContext injected for database access.
    public class MedicalRecordAuditLogRepository(IntegrityVaultDbContext _context) : IMedicalRecordAuditLogRepository
    {
        // Method to get the version of medical rocord.
        public async Task<List<MedicalRecordAuditLog>> GetAllVersionOfMedicalRecordByID(int medicalID)
        {
            try
            {

            return await _context.MedicalRecordsAuditLogs
                .Where(log => log.RecordID == medicalID)
                .ToListAsync();
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database retriving error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while retriving medical record audit log data from the database: {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while retriving medical record audit log data {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while retriving medical record audit log data: {ex.Message}."); // Throw a custom exception for general errors during medical record audit log data retrival.
            }
        }

        // Method to insert a new audit log and return success/failure.
        public async Task<bool> InsertAuditLog(CreateMedicalRecordAuditDTO createMedicalRecordAuditDTO)
        {
            try
            {
                var auditLog = new MedicalRecordAuditLog
                {
                    RecordID = createMedicalRecordAuditDTO.RecordID,
                    UpdatedByDoctorID = createMedicalRecordAuditDTO.UpdatedByDoctorID,
                    PreviousIPFS_CID = createMedicalRecordAuditDTO.PreviousIPFS_CID,
                    NewIPFS_CID = createMedicalRecordAuditDTO.NewIPFS_CID,
                    Version = createMedicalRecordAuditDTO.Version,
                    UpdatedAt = createMedicalRecordAuditDTO.UpdatedAt,
                    NewContentHash = createMedicalRecordAuditDTO.NewContentHash,
                    NewVersionHash = createMedicalRecordAuditDTO.NewVersionHash,
                    PreviousContentHash = createMedicalRecordAuditDTO.PreviousContentHash,
                    PreviousVersionHash = createMedicalRecordAuditDTO.PreviousVersionHash

                };

            // Insert the new record into the database.
                await _context.MedicalRecordsAuditLogs.AddAsync(auditLog);
            return true; // Return true if insertion was successful.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during the medical record audit log creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a medical record audit log {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new medical record audit log{ex.Message}."); // Throw a custom exception for general errors during medical record audit log creation.
            }
        }


        // Method to returns the audit log for a CID that points to a newer version of a medical record.
        public async Task<MedicalRecordAuditLog?> GetAuditLogByNewCIDAsync(string cid)
        {
            if (string.IsNullOrWhiteSpace(cid))
            {
                return null;
            }

            return await _context.MedicalRecordsAuditLogs.AsNoTracking()
                .FirstOrDefaultAsync(a => a.NewIPFS_CID == cid);
        }


        // Returns the audit log for a CID that points to an older version of a medical record.
        public async Task<MedicalRecordAuditLog?> GetAuditLogByPreviousCIDAsync(string cid)
        {
            if (string.IsNullOrWhiteSpace(cid))
            {
                return null;
            }

            return await _context.MedicalRecordsAuditLogs.AsNoTracking()
                                   .FirstOrDefaultAsync(a => a.PreviousIPFS_CID == cid);
        }
    }
}
