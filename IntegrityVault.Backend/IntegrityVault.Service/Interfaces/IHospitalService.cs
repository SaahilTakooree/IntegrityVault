// Import dependencies.
using IntegrityVault.Common.DTOs; // Importing the data transfer objects (DTOs) used for hospital creation and interaction.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IHospitalservice interface, which will be implemented by the hospital service.
    public interface IHospitalService
    {
        Task<IEnumerable<HospitalDTO>> GetAllHospitalsAsync(); // Returns every hospital mapped to a role-specific DTO.
        Task<HospitalDTO?> GetHospitalByIdAsync(int id); // Method signature for fetching a hospital by ID asynchronously. Returns a nullable Hospital object.
        Task<bool> CreateHospitalAsync(CreateHospitalDTO createHospitalDTO); // Method signature for creating a hospital asynchronously. Returns a boolean indicating success.
        Task<bool> UpdateHospitalAsync(int id, UpdateHospitalDTO updateHospitalDTO); // Validates and applies changes to an existing hospital record.
        Task<bool> DeleteHospitalAsync(int id); // Removes a hospital by primary key.
    }
}
