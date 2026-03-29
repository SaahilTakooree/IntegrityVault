// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO to view a medical record.
    public class MedicalRecordViewingItemDTO
    {
        public required string DisplayName { get; set; }
        public required string IPFS_CID { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp  { get; set; }
    }


    // One access log entry shown on the UI.
    public class RecordAccessLogItemDTO
    {
        public required string AccessType { get; set; }
        public required string AccessedByName { get; set; }
        public required string AccessedByRole { get; set; }
        public DateTime Timestamp { get; set; }
    }


    // DTO to show the detail of the medical record.
    public class MedicalRecordDetailDTO
    {
        public int MedicalRecordID { get; set; }
        public DateOnly VisitDate { get; set; }
        public int CurrentVersion { get; set; }
        public List<MedicalRecordViewingItemDTO> Versions { get; set; } = [];
        public List<RecordAccessLogItemDTO> AccessLogs { get; set; } = [];
    }


    // DTO to show the detail of the episode.
    public class EpisodeDetailDTO
    {
        public int EpisodeID { get; set; }
        public required string ChiefComplaint { get; set; }
        public bool IsActive { get; set; }
        public List<MedicalRecordDetailDTO> Records { get; set; } = [];
    }


    // DTO to group the epsidoe by speciality.
    public class SpecialityGroupDTO
    {
        public required string Speciality { get; set; }
        public List<EpisodeDetailDTO> Episodes { get; set; } = [];
    }


    // DTO for Full patient history response.
    public class PatientMedicalHistoryDTO
    {
        public int PatientID { get; set; }
        public required string PatientFullName { get; set; }
        public List<SpecialityGroupDTO> Specialities { get; set; } = [];
    }


    // DTO for a patient entry in the doctor's view,.
    public class DoctorPatientSummaryDTO
    {
        public int PatientID { get; set; }
        public required string PatientFullName { get; set; }
        public List<EpisodeDetailDTO> Episodes { get; set; } = [];
    }


    // DTO for full doctor history response.
    public class DoctorMedicalHistoryDTO
    {
        public int DoctorID { get; set; }
        public required string DoctorFullName { get; set; }
        public List<DoctorPatientSummaryDTO> Patients { get; set; } = [];
    }
}