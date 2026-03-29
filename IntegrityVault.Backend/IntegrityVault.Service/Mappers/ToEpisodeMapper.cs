// Import dependencies.
using IntegrityVault.Common.DTOs; // Data Transfer Objects for communication.
using IntegrityVault.Common.Entities; // Entity models representing the domain objects.


// Declaring the namespace where the mappers resides.
namespace IntegrityVault.Service.Mappers
{
    // Mapper class to convert data to a Episode entity.
    internal static class ToEpisodeMapper
    {
        // Method to map the relevant fields to a Episode entity.
        internal static Episode ToEpisodeEntity(CreateMedicalRecordDTO createMedicalRecordDTO, DateTime currentTime)
        {
            // Return a new Episode entity, with all necessary data populated.
            return new Episode
            {
                PatientID = createMedicalRecordDTO.PatientID,
                DoctorID = createMedicalRecordDTO.DoctorID,
                Specialty = createMedicalRecordDTO.Specialty,
                Title = createMedicalRecordDTO.ChiefComplaint,
                CreatedAt = currentTime
            };
        }
    }
}