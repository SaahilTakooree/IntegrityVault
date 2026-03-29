// Import dependencies.
using IntegrityVault.Common.DTOs; // Importing the data transfer objects (DTOs) used for the creation of a medical record.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IMedicalRecordService interface, which will be implemented by the medical record service.
    public interface IMedicalRecordService
    {
        Task<bool> CreateMedicalRecordAndEpisodeAsync(CreateMedicalRecordDTO createMedicalRecordDTO); // Method to create a new medical record with an epiode.
        Task<bool> AddMedicalRecordToEpisodeAsync(int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO); // Method to add medical record to an episode.
        Task<bool> PatchMedicalRecordAsync(int medicalRecordID, int episodeID, CreateMedicalRecordDTO createMedicalRecordDTO); // Method to update a medical record.
        Task<PatientMedicalHistoryDTO> GetPatientMedicalHistoryAsync(int patientID); // Method to get the full medical record history for a patient.
        Task<DoctorMedicalHistoryDTO>  GetDoctorMedicalHistoryAsync(int doctorID); // Method to get the full medical record for all the patients of a doctor.
        Task<VerifyMedicalRecordDTO> IsMedicalRecordTamperedAsync(string cid, int userID); // Method to verify a medical record by its cid.
        Task<VerifyMedicalRecordDTO> VerifyPdfTamperingAsync(byte[] pdfBytes, int userID); // Method to check if pdf is has not been tampered with.
        Task<MedicalRecordPdfDataDTO> GetMedicalRecordInformationFromCIDAsync(string cid, int userID); // Method to get the infomration out of the medical record.
        Task<(byte[] pdfBytes, string fileName)> DownloadMedicalRecordAsync(string cid, int userID); // Method to download a medical record.
    }
}
