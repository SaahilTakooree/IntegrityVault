// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for medical record data, such as MedicalRecordIdDTO.
using IntegrityVault.Common.Entities; // Imported to allow access to the MedicalRecord entity.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IMedicalRecordRepository interface that represents the contract for medical record related database operations.
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord?> GetMedicalRecordById(int medicalRecordID); // Method to get a medical record by its id. Return null if does not exist.
        Task<MedicalRecordIdDTO> CreateMedicalRecordAsync(MedicalRecord medicalRecord); // Method to create a medical record. return ture if it was successful.
        Task<bool> PatchMedicalRecordAsync(int medicalRecordID, MedicalRecordPatchDTO medicalRecordPatchDTO); // Method to pathch a medical record. Return false if it was successful.
        Task<List<MedicalRecord>> GetMedicalRecordsByPatientIDAsync(int patientID); // Method to get all the medical record that patient has.
        Task<List<MedicalRecord>> GetMedicalRecordsByDoctorIDAsync(int doctorID); // Method to get all the medical record that a doctor is assosicated with.
        Task<MedicalRecord?> GetMedicalRecordByCIDAsync(string cid); // Method to get a medical record by a CID.
    }
}
