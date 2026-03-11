// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO for login a user.
    public class AuthLoginDTO
    {
        public required string UsernameOrEmail { get; set; }
        public required string Password { get; set; }
        public required string IpAddress { get; set; }

    }
}
