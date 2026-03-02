// Import dependencies needed.
using IntegrityVault.Repository.Interfaces; // Import the interface for the hospital repository.
using IntegrityVault.Service.Interfaces; // Import the interface for the hospital service.
using IntegrityVault.Common.Entities; // Import the entity class for Hospital.
using IntegrityVault.Common.Helpers; // Import helper functions.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used in the service layer.

// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the HospitalService class and injecting the IHospitalRepository dependency.
    public class HospitalService(IHospitalRepository _hospitalRepository) : IHospitalService
    {
        // Method to return all hospitals mapped to their role-specific DTOs.
        public async Task<IEnumerable<HospitalDTO>> GetAllHospitalsAsync()
        {
            try
            {
                // Get all the hospitals from the repository.
                var hospitals = await _hospitalRepository.GetAllHospitalsAsync();

                // Map each Hospital entity to the most specific DTO avaliable for that role.
                return hospitals.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching all hospitals: {ex.Message}.");
            }
        }

        // Method to fetch a hospital by their ID asynchronously.
        public async Task<HospitalDTO?> GetHospitalByIdAsync(int id)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Hospital Id");

                // Fetch the hospital from the repository by ID.
                var hospital = await _hospitalRepository.GetHospitalByIdAsync(id);

                // Return null when the hospital does not exist.
                if (hospital is null) return null;

                return MapToDTO(hospital);
            }
            catch (Exception ex) // Catching any general exceptions.
            {
                throw new InvalidOperationException($"Error fetching hospital by ID: {ex.Message}."); // Wrapping the exception and throwing it with a custom message.
            }
        }


        // Method to create a hospital asynchronously.
        public async Task<bool> CreateHospitalAsync(CreateHospitalDTO createHospitalDTO)
        {
            try
            {
                // Check if the wallet already exists.
                if (await DoesWalletAddressExist(createHospitalDTO.WalletAddress) != null) // Checking if a hospital with the same wallet already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createHospitalDTO.WalletAddress}' already exists."); // Throwing an error if the wallet address is already taken.
                }

                // Create the hospital.
                return await _hospitalRepository.CreateHospitalAsync(createHospitalDTO); // Create the hospital in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"Hospital creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for hospital creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during hospital creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to update the hospital repository.
        public async Task<bool> UpdateHospitalAsync(int id, UpdateHospitalDTO updateHospitalDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Hospital Id");

                // Check if the wallet already exists.
                if (updateHospitalDTO.WalletAddress is not null && await DoesWalletAddressExist(updateHospitalDTO.WalletAddress) != null) // Checking if a hospital with the same wallet already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updateHospitalDTO.WalletAddress}' already exists."); // Throwing an error if the wallet address is already taken.
                }

                return await _hospitalRepository.UpdateHospitalAsync(id, updateHospitalDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Hospital update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during Hospital update: {ex.Message}.");
            }
        }


        // Method to delete a hospital from repository.
        public async Task<bool> DeleteHospitalAsync(int id)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Hospital Id");

                return await _hospitalRepository.DeleteHospitalAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Hosptal delete failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during hospital deletion: {ex.Message}.");
            }
        }


        // Private method to map the hospital to its DTO.
        private static HospitalDTO MapToDTO(Hospital hospital)
        {
            return new HospitalDTO
            {
                ID = hospital.ID,
                Name = hospital.Name,
                WalletAddress = hospital.WalletAddress
            };
        }


        // Private method to check if an wallet address already exists in the database.
        private async Task<Hospital?> DoesWalletAddressExist(string walletAddress)
        {
            try
            {
                // Fetch all hospitals from the repository.
                var allHospitals = await _hospitalRepository!.GetAllHospitalsAsync();

                // Search for the user with the matching wallet address and returning the first match or null.
                return allHospitals.FirstOrDefault(h => h.WalletAddress == walletAddress);
            }
            catch (Exception ex) // Catch any exceptions that may occur during the check.
            {
                throw new InvalidOperationException($"Error checking if email exists: {ex.Message}."); // Wrap the exception and throwing it with a custom message for wallet address existence check failure.
            }
        }
    }
}