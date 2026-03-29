// Import dependencies.
using IntegrityVault.Common.Enums; // Make the doctor speciality enum avaliable to be use in the enum.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO to create an episode.
    public class CreateEpisodeDTO
    {
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public DoctorSpecialty Specialty { get; set; }
        public required string Title { get; set; }
    }


    // DTO to return just the ID of the created episode.
    public class EpisodeIdDTO
    {
        public int ID { get; set; }
    }
}