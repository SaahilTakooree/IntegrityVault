// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for hospital data, such as CreateHospitalDTO.
using IntegrityVault.Common.Entities; // Import the entity class for Hospital.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IHospitalRepository interface that represents the contract for hospital-related database operations.
    public interface IHospitalRepository
    {
        Task<IEnumerable<Hospital>> GetAllHospitalsAsync(); // Method signature for fetching all hospitals asynchronously. Returns a list of Hospital entities.
        Task<Hospital?> GetHospitalByIdAsync(int id); // Method signature for fetching a single hospital by their ID asynchronously. Returns a nullable Hospital object.
        Task<bool> CreateHospitalAsync(CreateHospitalDTO createHospitalDTO); // Method signature for creating a new hospital asynchronously. Accepts a CreateHospitalDTO and returns a boolean indicating success.
        Task<bool> UpdateHospitalAsync(int id, UpdateHospitalDTO updateHospitalDTO); // Updates an Hospital record identified by the given ID.
        Task<bool> IsIpAuthorisedAsync(int hospitalId, string ipAddress); // Method to check if an ipaddress actually belong to that hospital.
        Task<bool> DeleteHospitalAsync(int id); // Deletes the hospital by primary key.
    }
}
