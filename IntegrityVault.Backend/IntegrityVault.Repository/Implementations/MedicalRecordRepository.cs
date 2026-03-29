// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IMedicalRecordRepository interface to implement the repository.
using IntegrityVault.Common.Entities; // Import the entity classes representing MedicalRecord.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.
using IntegrityVault.Common.DTOs; // Import data transfer objects used in the repository for medical record creation.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IMedicalRecordRepository interface, with the DbContext injected for database access.
    public class MedicalRecordRepository(IntegrityVaultDbContext _context) : IMedicalRecordRepository
    {
        // Method to get a medical record by its id. Return null if does not exist.
        public async Task<MedicalRecord?> GetMedicalRecordById(int medicalRecordID)
        {
            try
            {
                // Finding the medical record by ID asynchronously, returning null if not found.
                return await _context!.MedicalRecords
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ID == medicalRecordID);
            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                {
                    Console.WriteLine($"Error while retrieving medical by ID {medicalRecordID} {ex.Message}."); // Log the error message to the console.
                    throw new InvalidOperationException($"Error retrieving medical with ID {medicalRecordID} from the database {ex.Message}"); // Throw a custom exception with the error message.
                }
            }
        }


        // Method to add a medical record to the database.
        public Task<MedicalRecordIdDTO> CreateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            try
            {
                // Save changes and return true if successful.
                _context.MedicalRecords.Add(medicalRecord);

                return Task.FromResult(new MedicalRecordIdDTO { ID = medicalRecord.ID }); // Return the ID to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during the medical record creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a medical record {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new medical record {ex.Message}."); // Throw a custom exception for general errors during medical record creation.
            }
        }


        // Method to pathch a medical record. Return false if it was successful.
        public async Task<bool> PatchMedicalRecordAsync(int medicalRecordID, MedicalRecordPatchDTO medicalRecordPatchDTO)
        {
            try
            {
                // Retrieve the medical record.
                var medicalRecord = await _context.MedicalRecords
                    .FirstOrDefaultAsync(m => m.ID == medicalRecordID)
                    ?? throw new InvalidOperationException("Medical record not found.");

                // Update only the provided fields in the DTO.
                if (!string.IsNullOrEmpty(medicalRecordPatchDTO.IPFS_CID))
                {
                    medicalRecord.IPFS_CID = medicalRecordPatchDTO.IPFS_CID;
                }

                if (medicalRecordPatchDTO.CurrentVersion.HasValue)
                {
                    medicalRecord.CurrentVersion = medicalRecordPatchDTO.CurrentVersion.Value;
                }

                if (medicalRecordPatchDTO.UpdatedAt.HasValue)
                {
                    medicalRecord.UpdatedAt = medicalRecordPatchDTO.UpdatedAt.Value;
                }

                if (!string.IsNullOrEmpty(medicalRecordPatchDTO.ContentHash))
                {
                    medicalRecord.ContentHash = medicalRecordPatchDTO.ContentHash;
                }

                if (!string.IsNullOrEmpty(medicalRecordPatchDTO.VersionHash))
                {
                    medicalRecord.VersionHash = medicalRecordPatchDTO.VersionHash;
                }

                medicalRecord.PreviousVersionHash = medicalRecordPatchDTO.PreviousVersionHash;

                if (!string.IsNullOrEmpty(medicalRecordPatchDTO.BlockchainTxHash))
                {
                    medicalRecord.BlockchainTxHash = medicalRecordPatchDTO.BlockchainTxHash;
                }


                // Return true to show that where not any errors yet.
                return true;
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during the medical record updating {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a medical record {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new medical record {ex.Message}."); // Throw a custom exception for general errors during medical record updating.
            }
        }


        // Method to get all the medical record that patient has.
        public async Task<List<MedicalRecord>> GetMedicalRecordsByPatientIDAsync(int patientID)
        {
            try
            {
                // Retrieve the medical record.
                return await _context.MedicalRecords
                    .AsNoTracking()
                    .Include(m => m.Episode)
                        .ThenInclude(e => e!.Doctor)
                    .Include(m => m.AuditLogs)
                    .Include(m => m.AccessLogs)
                    .Where(m => m.Episode!.PatientID == patientID)
                    .ToListAsync();
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"Error retrieving records for patient {patientID}: {ex.Message}"); // Log a general error message.
                throw new InvalidOperationException($"Error retrieving records for patient {patientID}: {ex.Message}"); // Throw a custom exception for general errors during medical record retrival.
            }
        }


        // Method to get all the medical record that a doctor is assosicated with.
        public async Task<List<MedicalRecord>> GetMedicalRecordsByDoctorIDAsync(int doctorID)
        {
            try
            {
                // Retrieve the medical record.
                return await _context.MedicalRecords
                    .AsNoTracking()
                    .Include(m => m.Episode)
                    .Include(m => m.AuditLogs)
                    .Include(m => m.AccessLogs)
                    .Where(m => m.Episode!.DoctorID == doctorID)
                    .ToListAsync();
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"Error retrieving records for doctor {doctorID}: {ex.Message}"); // Log a general error message.
                throw new InvalidOperationException($"Error retrieving records for doctor {doctorID}: {ex.Message}"); // Throw a custom exception for general errors during medical record retrival.
            }
        }


        // Method to get a medical record by a CID.
        public async Task<MedicalRecord?> GetMedicalRecordByCIDAsync(string cid)
        {
            if (string.IsNullOrWhiteSpace(cid))
            {
                return null;
            }

            // Search for a record where the current IPFS_CID matches the given CID.
            var record = await _context.MedicalRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IPFS_CID == cid);

            return record;
        }
    }
}
