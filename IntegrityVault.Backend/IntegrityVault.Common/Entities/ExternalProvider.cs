// Import dependencies.
using System.ComponentModel.DataAnnotations; // Import attributes for data validation.
using System.ComponentModel.DataAnnotations.Schema; // Import attributes for data schema mapping.


// Define the namespace for the entity classes in the IntegrityVault project.
namespace IntegrityVault.Common.Entities
{
    // Declare the class external provider.
    public class ExternalProvider : User
    {
        // Foreign key to reference the hospital that can use the external provider account.
        [Required]
        public int BelongsToID { get; set; }
        // Navigation property to the hospital that owns this exteral provider.
        [ForeignKey("BelongsToID")]
        public Hospital? BelongsTo { get; set; }
    }
}
