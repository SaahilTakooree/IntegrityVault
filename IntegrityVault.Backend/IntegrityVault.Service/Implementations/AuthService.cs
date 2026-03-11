// Import dependencies needed.
using IntegrityVault.Repository.Interfaces; // Import the interface for the auth repository.
using IntegrityVault.Service.Interfaces; // Import the interface for the auth service.
using IntegrityVault.Common.Entities; // Import the user entity.
using IntegrityVault.Common.Enums; // Import the entity class for User.


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the AuthService class and injecting the IAuthRepository and IHospitalRepository dependency.
    public class AuthService(IAuthRepository _authRepository, IHospitalRepository _hospitalRepository) : IAuthService
    {
        // Method to return the a user if credential matches.
        public async Task<User?> LoginAsync(string usernameOrEmail, string password, string ipAddress)
        {
            try
            {
                // Get the user from the database.
                var user = await _authRepository.GetUserByCredentialAsync(usernameOrEmail, password);

                // If the user does not exist, return null.
                if (user == null)
                    return null;

                // Check if user ip address is correct to allow login.
                var ipAllowed = user.Role switch
                {
                    // IP checking is not needed if role is superadmin or patient.
                    UserRole.SuperAdmin or UserRole.Patient => true,

                    // If role is admin or doctor, ip address must match to their assign hospital.
                    UserRole.Admin or UserRole.Doctor =>
                        user.HospitalID.HasValue && await _hospitalRepository.IsIpAuthorisedAsync(user.HospitalID.Value, ipAddress),

                    // If user is not tie to any hospital.
                    UserRole.ExternalProvider =>
                        user is ExternalProvider externalProvider && await _hospitalRepository.IsIpAuthorisedAsync(externalProvider.BelongsToID, ipAddress),

                    // Deny all other unknow values
                    _ => false
                };
                
                // Return the user
                return ipAllowed ? user : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while trying to fetch to log a user in: {ex.Message}."); // Wrapping the exception and throwing it with a custom message.
            }
        }
    }
}
