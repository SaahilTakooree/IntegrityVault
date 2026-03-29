// Import dependencies.
using IntegrityVault.Common.Enums; // Make the doctor speciality enum avaliable to be use in the enum.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO of medical record pdf.
    public class MedicalRecordPdfDataDTO
    {
        public int EpisodeID { get; set; }
        public int MedicalRecordVersion { get; set; } = 1;
        public int  PatientID { get; set; }
        public required string  PatientFirstName { get; set; }
        public string?  PatientMiddleName { get; set; }
        public required string  PatientLastName { get; set; }
        public required PatientGender PatientGender { get; set; }
        public DateOnly PatientDOB {  get; set; }
        public int DoctorID { get; set; }
        public required string DoctorFirstName { get; set; }
        public string? DoctorMiddleName { get; set; }
        public required string DoctorLastName { get; set; }
        public required DoctorSpecialty DoctorSpecialy { get; set; }
        public int HospitalID { get; set; }
        public required string HospitalName { get; set; }
        public DateOnly VisitDate { get; set; }
        public required string ChiefComplaint { get; set; }
        public required string Diagnosis { get; set; }
        public required string TreatmentPlan { get; set; }
        public required string DoctorNotes { get; set; }
        public required string FollowUpInstructions { get; set; }
        public int Version { get; set; } = 1;
    }
}