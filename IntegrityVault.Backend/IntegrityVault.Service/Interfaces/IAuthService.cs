// Import dependencies.
using IntegrityVault.Common.Entities; // Importing the user entity.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IAuthservice interface, which will be implemented by the authentication service.
    public interface IAuthService
    {
        Task<User?> GetUserByEmailAndPasswordAsync(string email, string password);
    }
}
