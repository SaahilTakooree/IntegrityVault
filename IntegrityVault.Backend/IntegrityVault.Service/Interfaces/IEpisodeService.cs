// Import dependencies.
using IntegrityVault.Common.DTOs; // Importing the data transfer objects (DTOs) used for Episode creation and interaction.
using IntegrityVault.Common.Entities; // Import the Episode entity.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IEpisode service interface, which will be implemented by the episode service.
    public interface IEpisodeService
    {
        Task<Episode?> GetEpisodeByIdAsync(EpisodeIdDTO episodeIdDTO); // Method to check if an episode exist by its ID.
        Task<EpisodeIdDTO> CreateEpisodeAsync(Episode Episode); // Method signature for creating a new episode. Returns the Id of the created episode indicating success.
        Task<bool> SetEpisodeStatusAsync(int episodeID, int doctorID); // Method that switch the isActive status form an epiosde.
    }
}
