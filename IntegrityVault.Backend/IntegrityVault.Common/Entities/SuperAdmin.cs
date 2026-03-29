// Define the namespace for the entity classes in the IntegrityVault project.
using System.ComponentModel.DataAnnotations;

namespace IntegrityVault.Common.Entities
{
    // Declare the class super admin.
    public class SuperAdmin : User
    {
        // Superadmin's blockchain wallet address. It is required and must be exactly 42 characters long.
        [Required]
        [StringLength(42, MinimumLength = 42, ErrorMessage = "Wallet address must exactly 42 characters long.")]
        public required string WalletAddress { get; set; }

        // Superadmin private key encrypted.
        [Required]
        public required byte[] EncryptedPrivateKey { get; set; }
    }
}
