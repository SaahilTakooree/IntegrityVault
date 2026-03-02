// Import dependencies that is needed to create the doctor entity.
using System.ComponentModel.DataAnnotations; // Import attributes for data validation.
using IntegrityVault.Common.Enums; // Import custom enums from the project.


// Define the namespace for the entity classes in the IntegrityVault project.
namespace IntegrityVault.Common.Entities
{
    // Declare the class doctor.
    public class Doctor : User
    {
        // Doctor's first name. It is required and limited to 100 characters.
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        // Doctor's middle name. It is optional and limited to 100 characters.
        [StringLength(100)]
        public string? MiddleName { get; set; }

        // Doctor's last name. It is required and limited to 100 characters.
        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        // Doctor' specialty. It is required.
        [Required]
        public required DoctorSpecialty Specialty { get; set; }

        // Navigation property representing all medical record associated with this patient.
        public virtual ICollection<MedicalRecord> CreatedRecords { get; set; } = new HashSet<MedicalRecord>();
    }
}
