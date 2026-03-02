// Import dependencies needed.
using IntegrityVault.Repository.Interfaces; // Import the interface for the auth repository.
using IntegrityVault.Service.Interfaces; // Import the interface for the auth service.
using IntegrityVault.Common.Entities; // Import the entity class for User.


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the AuthService class and injecting the IAuthRepository dependency.
    public class AuthService(IAuthRepository _authRepository) : IAuthService
    {
        // Method to return the a user if credential matches.
        public async Task<User?> GetUserByEmailAndPasswordAsync(string usernameOrEmail, string password)
        {
            try
            {
                // Get the user from the database.
                var user = await _authRepository.GetUserByEmailAndPasswordAsync(usernameOrEmail, password);

                // If the user does not exist, return null.
                if (user == null)
                    return null;
                
                // Return the user
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while trying to fetch to log a user in: {ex.Message}."); // Wrapping the exception and throwing it with a custom message.
            }
        }
    }
}
