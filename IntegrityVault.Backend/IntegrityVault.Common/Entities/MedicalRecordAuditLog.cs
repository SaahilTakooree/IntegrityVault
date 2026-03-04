// Import dependencies that is needed to create the medical record audit log entity.
using System.ComponentModel.DataAnnotations; // Import attributes for data validation.
using System.ComponentModel.DataAnnotations.Schema; // Import attributes for data schema mapping.


// Define the namespace for the entity class in the IntegrityVault project.
namespace IntegrityVault.Common.Entities
{
    // Define the class MedicalRecordAutditLog where that every time a doctor updates a medical record, a new row is inserted to preserves the full version history.
    public class MedicalRecordAuditLog
    {
        // Primary key for the MedicalRecordAuditLog table.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        // Foeign key referencing the medical record that was changed. It is required.
        [Required]
        public required int RecordID { get; set; }

        //Navigation property to the related medical record entity.
        [ForeignKey("RecordID")]
        public MedicalRecord? Record { get; set; }

        //Foreign key referencing the doctor who made the update. It is requied.
        [Required]
        public required int UpdatedByDoctorID { get; set; }

        // Navigation property to the doctor who made the update.
        [ForeignKey("UpdatedByDoctorID")]
        public Doctor? UpdatedByDoctor { get; set; }

        // The IPFS CID that was stored on the record before an update. It is required and must be between 40 and 90 characters.
        [Required]
        [StringLength(90, MinimumLength = 40, ErrorMessage = "Previous IPFS CID must be at least 40 characters.")]
        public required string PreviousIPFS_CID { get; set; }

        // The new IPFS CID that replaced the old one in this update. It is required and must be between 40 and 90 characters.
        [Required]
        [StringLength(90, MinimumLength = 40, ErrorMessage = "New IPFS CID must be at least 40 characters.")]
        public required string NewIPFS_CID { get; set; }

        // The version number of this entry for this record.
        [Required]
        public required int Version { get; set; }

        // Date and time when this update was made.
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
