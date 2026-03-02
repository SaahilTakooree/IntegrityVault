// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IAuthRepository interface to implement the repository.
using IntegrityVault.Common.Entities; // Import the entity classes representing the user.
using IntegrityVault.Common.Helpers; // Import helper functions.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IAuthRepository interface, with the DbContext injected for database access.
    public class AuthRepository(IntegrityVaultDbContext _context) : IAuthRepository
    {
        // Get the user from the repository then compare it to the credential
        public async Task<User?> GetUserByEmailAndPasswordAsync(string usernameOrEmail, string password)
        {
            // Try to see if the credital match one of the user in the database.
            var user = await _context!.Users.FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.Username == usernameOrEmail);

            // Return null if the user does not exist.
            if (user == null) return null;

            // Check if the password does exist.
            if (password is null)
                throw new InvalidOperationException("Password cannot be null.");
            
            // Verify the inputed password against the stored hashed password.
            var passwordMatches = HashHelper.VerifyInput(user.Password, password); // Hashing the password using a helper method and ensuring it is not null.

            // If password does not match, then login is failed.
            if (!passwordMatches)
                return null;

            return user;
        }
    }
}
