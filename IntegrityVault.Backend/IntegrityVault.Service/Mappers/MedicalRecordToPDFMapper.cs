// Import dependencies.
using IntegrityVault.Common.DTOs; // Data Transfer Objects for communication.
using IntegrityVault.Common.Entities; // Entity models representing the domain objects.


// Declaring the namespace where the mappers resides.
namespace IntegrityVault.Service.Mappers
{
    // Mapper class to convert entities to a MedicalRecordPdfDataDTO for PDF generation.
    internal static class MedicalRecordToPDFMapper
    {
        // Method to map the relevant fields from various entities to a MedicalRecordPdfDataDTO.
        internal static MedicalRecordPdfDataDTO ToPDFDataDTO(int episodeID, Patient patient, Doctor doctor,
            Hospital hospital, MedicalRecordDTO medicalRecordDTO, int version)
        {
            // Return a new MedicalRecordPdfDataDTO object, with all necessary data populated.
            return new MedicalRecordPdfDataDTO
            {
                EpisodeID = episodeID,
                Version = version,
                PatientID = patient!.ID,
                PatientFirstName = patient.FirstName,
                PatientMiddleName = patient.MiddleName,
                PatientLastName = patient.LastName,
                PatientGender = patient.Gender,
                PatientDOB = patient.DOB,
                DoctorID = doctor!.ID,
                DoctorFirstName = doctor.FirstName,
                DoctorMiddleName = doctor.MiddleName,
                DoctorLastName = doctor.LastName,
                DoctorSpecialy = doctor.Specialty,
                HospitalName = hospital!.Name,
                VisitDate = medicalRecordDTO.VisitDate,
                ChiefComplaint = medicalRecordDTO.ChiefComplaint,
                Diagnosis = medicalRecordDTO.Diagnosis,
                TreatmentPlan = medicalRecordDTO.TreatmentPlan,
                DoctorNotes = medicalRecordDTO.DoctorNotes ?? "",
                FollowUpInstructions = medicalRecordDTO.FollowUpInstructions ?? ""
            };
        }
    }
}