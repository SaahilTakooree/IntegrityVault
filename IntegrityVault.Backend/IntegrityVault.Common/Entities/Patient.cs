// Import dependencies that is needed to create the patient entity.
using System.ComponentModel.DataAnnotations; // Import attributes for data validation.
using IntegrityVault.Common.Enums; // Import custom enums from the project.


// Define the namespace for the entity classes in the IntegrityVault project.
namespace IntegrityVault.Common.Entities
{
    // Declare the class patient.
    public class Patient : User
    {
        // Patient's first name. It is required and limited to 100 characters.
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        // Patient's middle name. It is optional and limited to 100 characters.
        [StringLength(100)]
        public string? MiddleName { get; set; }

        // Patient's last name. It is required and limited to 100 characters.
        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        // Patient's date of birth. It is required.
        [Required]
        public required DateOnly DOB { get; set; }

        // Patient' gender. It is required.
        [Required]
        public required PatientGender Gender { get; set; }

        // Navigation property representing all medical record associated with this patient.
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new HashSet<MedicalRecord>();
    }
}