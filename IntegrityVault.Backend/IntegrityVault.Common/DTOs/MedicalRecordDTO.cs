// Import dependencies.
using IntegrityVault.Common.Enums; // Make the doctor speciality enum avaliable to be use in the enum.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO to of a medical record.
    public class MedicalRecordDTO
    {
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public DoctorSpecialty Specialty { get; set; }
        public DateOnly VisitDate { get; set; }
        public required string ChiefComplaint { get; set; }
        public required string Diagnosis { get; set; }
        public required string TreatmentPlan { get; set; }
        public string? DoctorNotes { get; set; }
        public string? FollowUpInstructions { get; set; }
    }


    // DTO to create a new medical record.
    public class CreateMedicalRecordDTO : MedicalRecordDTO
    {
    }


    // DTO to add a medical record to an existig episode.
    public class AddMedicalRecordToEpisodeDTO : MedicalRecordDTO
    {
        public int EpisodeID { get; set; }
    }


    // DTO to return just the ID of a medical record.
    public class MedicalRecordIdDTO
    {
        public int ID { get; set; }
    }


    // DTO to patch the medical record.
    public class MedicalRecordPatchDTO
    {
        public string? IPFS_CID { get; set; }
        public int? CurrentVersion { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ContentHash { get; set; }
        public string? VersionHash { get; set; }
        public string? PreviousVersionHash { get; set; }
        public string? BlockchainTxHash { get; set; }
    }
}
