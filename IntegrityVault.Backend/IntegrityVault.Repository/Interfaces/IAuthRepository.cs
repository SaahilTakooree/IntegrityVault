// Import dependencies.
using IntegrityVault.Common.Entities; // Import the entity class for User.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IAuthRepository interface that represents the contract for auth-related database operation.
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAndPasswordAsync(string email, string password);
    }
}
