// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for user data, such as CreateUserDTO.
using IntegrityVault.Common.Entities; // Import the entity class for User.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces 
{
    // Define the IUserRepository interface that represents the contract for user-related database operations.
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(int? hospitalID = null); // Method signature for fetching all users asynchronously. Returns a list of User entities.
        Task<User?> GetUserByIdAsync(int id); // Method signature for fetching a single user by their ID asynchronously. Returns a nullable User object.
        Task<IEnumerable<Patient>> GetAllPatientsFromHospitalAsync(int hospitalID); // Method to fetch all patients from a specific hospital asynchronously.
        Task<Admin?> GetAdminByIdAsync(int id); // Method signature for fetching a single admin by their ID asynchronously. Returns a nullable admin object.
        Task<Doctor?> GetDoctorByIdAsync(int id); // Method signature for fetching a single doctor by their ID asynchronously. Returns a nullable doctor object.
        Task<ExternalProvider?> GetExternalProviderByIdAsync(int id); // Method signature for fetching a single external provider by their ID asynchronously. Returns a nullable external provider object.
        Task<Patient?> GetPatientByIdAsync(int id); // Method signature for fetching a single patient by their ID asynchronously. Returns a nullable patient object.
        Task<SuperAdmin?> GetSuperAdminByIdAsync(int id); // Method signature for fetching a single super admin by their ID asynchronously. Returns a nullable super admin object.
        Task<SuperAdmin?> GetSuperAdminByWalletAsync(string walletAddress); // Method signature for fetching a single super admin by their wallet address asynchronously. Returns a nullable super admin object.
        Task<bool> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO); // Method signature for creating a new user asynchronously. Accepts a CreateDoctorDTO and returns a boolean indicating success.
        Task<bool> CreatePatientAsync(CreatePatientDTO createPatientDTO); // Method signature for creating a new user asynchronously. Accepts a CreatePatientDTO and returns a boolean indicating success.
        Task<bool> CreateAdminAsync(CreateAdminDTO createAdminDTO); // Method signature for creating a new user asynchronously. Accepts a CreateAdminAsync and returns a boolean indicating success.
        Task<bool> CreateExternalProviderAsync(CreateExternalProviderDTO createExternalProviderDTO); // Method signature for creating a new user asynchronously. Accepts a CreateExternalProviderAsync and returns a boolean indicating success.
        Task<bool> CreateSuperAdminAsync(CreateSuperAdminDTO createSuperAdminDTO, byte[] encryptedKey); // Method signature for creating a new user asynchronously. Accepts a CreateSuperAdminDTO and returns a boolean indicating success.
        Task<bool> UpdateDoctorAsync(int id, UpdateDoctorDTO updateDoctorDTO); // Updates a Doctor record identified by the given ID.
        Task<bool> UpdatePatientAsync(int id, UpdatePatientDTO updatePatientDTO); // Updates a Patient record identified by the given ID.
        Task<bool> UpdateAdminAsync(int id, UpdateAdminDTO updateAdminDTO); // Updates an Admin record identified by the given ID.
        Task<bool> UpdateExternalProviderAsync(int id, UpdateExternalProviderDTO updateExternalProviderDTO); // Updates an ExternalProvider record identified by the given ID.
        Task<bool> UpdateSuperAdminAsync(int id, UpdateSuperAdminDTO updateSuperAdminDTO, byte[]? encryptedKey); // Updates an SuperAdmin record identified by the given ID.
        Task<bool> DeleteUserAsync(int id); // Deletes the user by primary key.
        Task<List<Doctor>> GetDoctorsByIDsAsync(List<int> doctorIDs); // Method to get all the doctor IDs.
        Task<List<Patient>> GetPatientsByIDsAsync(List<int> patientIDs); // Methdo to get all patient IDs.
    }
}