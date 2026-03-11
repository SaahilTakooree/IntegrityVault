// Import dependencies.
using IntegrityVault.Common.DTOs; // Importing the data transfer objects (DTOs) used for user creation and interaction.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IUserService interface, which will be implemented by the user service.
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync(int? hospitalId = null); // Returns every user mapped to a role-specific DTO.
        Task<UserDTO?> GetUserByIdAsync(int id); // Method signature for fetching a user by ID asynchronously. Returns a nullable User object.
        Task<bool> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO); // Method signature for creating a doctor asynchronously. Returns a boolean indicating success.
        Task<bool> CreatePatientAsync(CreatePatientDTO createPatientDTO); // Method signature for creating a patient asynchronously. Returns a boolean indicating success.
        Task<bool> CreateAdminAsync(CreateAdminDTO createAdminDTO); // Method signature for creating an admin asynchronously. Returns a boolean indicating success.
        Task<bool> CreateExternalProviderAsync(CreateExternalProviderDTO createExternalProviderDTO); // Method signature for creating an external provider asynchronously. Returns a boolean indicating success.
        Task<bool> CreateSuperAdminAsync(CreateSuperAdminDTO createSuperAdminDTO); // Method signature for creating a super admin asynchronously. Returns a boolean indicating success.
        Task<bool> UpdateDoctorAsync(int id, UpdateDoctorDTO updateDoctorDTO); // Validates and applies changes to an existing doctor record.
        Task<bool> UpdatePatientAsync(int id, UpdatePatientDTO updatePatientDTO); // Validates and applies changes to an existing patient record.
        Task<bool> UpdateAdminAsync(int id, UpdateAdminDTO updateAdminDTO); // Validates and applies changes to an existing admin record.
        Task<bool> UpdateExternalProviderAsync(int id, UpdateExternalProviderDTO updateExternalProviderDTO); // Validates and applies changes to an existing external provider record.
        Task<bool> UpdateSuperAdminAsync(int id, UpdateSuperAdminDTO updateSuperAdminDTO); // Validates and applies changes to an existing super admin record.
        Task<bool> DeleteUserAsync(int id); // Removes a user by primary key.
    }
}