// Import dependencies.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used for episode data, such as CreateEpisodeDTO.
using IntegrityVault.Common.Entities; // Imported to allow access to the Episode entity.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IEpisodeRepository interface that represents the contract for episode related database operations.
    public interface IEpisodeRepository
    {
        Task<Episode?> GetEpisodeByIdAsync(EpisodeIdDTO episodeIdDTO); // Method to get an episode by its ID.
        Task<EpisodeIdDTO> CreateEpisodeAsync(Episode Episode); // Method signature for creating a new episode. Returns the Id of the created episode indicating success.
        Task<bool> SetEpisodeStatusAsync(int episodeID); // Method that switch the isActive status form an epiosde.
        Task<bool> IsEpisodeActiveAsync(int episodeId); // Method to check if an epsiode is active of not.
    }
}
