// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for user data, such as CreateUserDTO.
using IntegrityVault.Common.Entities; // Import the entity class for User.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces 
{
    // Define the IUserRepository interface that represents the contract for user-related database operations.
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(); // Method signature for fetching all users asynchronously. Returns a list of User entities.
        Task<User?> GetUserByIdAsync(int id); // Method signature for fetching a single user by their ID asynchronously. Returns a nullable User object.
        Task<bool> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO); // Method signature for creating a new user asynchronously. Accepts a CreateDoctorDTO and returns a boolean indicating success.
        Task<bool> CreatePatientAsync(CreatePatientDTO createPatientDTO); // Method signature for creating a new user asynchronously. Accepts a CreatePatientDTO and returns a boolean indicating success.
        Task<bool> CreateAdminAsync(CreateAdminDTO createAdminDTO); // Method signature for creating a new user asynchronously. Accepts a CreateAdminAsync and returns a boolean indicating success.
        Task<bool> CreateExternalProviderAsync(CreateExternalProviderDTO createExternalProviderDTO); // Method signature for creating a new user asynchronously. Accepts a CreateExternalProviderAsync and returns a boolean indicating success.
        Task<bool> CreateSuperAdminAsync(CreateSuperAdminDTO createSuperAdminDTO); // Method signature for creating a new user asynchronously. Accepts a CreateSuperAdminDTO and returns a boolean indicating success.
        Task<bool> UpdateDoctorAsync(int id, UpdateDoctorDTO updateDoctorDTO); // Updates a Doctor record identified by the given ID.
        Task<bool> UpdatePatientAsync(int id, UpdatePatientDTO updatePatientDTO); // Updates a Patient record identified by the given ID.
        Task<bool> UpdateAdminAsync(int id, UpdateAdminDTO updateAdminDTO); // Updates an Admin record identified by the given ID.
        Task<bool> UpdateExternalProviderAsync(int id, UpdateExternalProviderDTO updateExternalProviderDTO); // Updates an ExternalProvider record identified by the given ID.
        Task<bool> UpdateSuperAdminAsync(int id, UpdateSuperAdminDTO updateSuperAdminDTO); // Updates an SuperAdmin record identified by the given ID.
        Task<bool> DeleteUserAsync(int id); // Deletes the user by primary key.
    }
}