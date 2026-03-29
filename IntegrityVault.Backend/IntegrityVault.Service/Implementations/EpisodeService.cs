// Import dependencies needed.
using IntegrityVault.Repository.Contexts; // Make the context avaliable for use.
using IntegrityVault.Repository.Interfaces; // Import the interface for the episode repository.
using IntegrityVault.Service.Interfaces; // Import the interface for the episode service.
using IntegrityVault.Common.Entities; // Import the entity class for Episode.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used in the service layer.
using IntegrityVault.Common.Helpers; // Import the helper function.


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the EpisodeService class and injecting the IEpisodeRepository and context dependency.
    public class EpisodeService(IEpisodeRepository _episodeRepository, IntegrityVaultDbContext _context) : IEpisodeService
    {

        // Method to check if an episode exist by its ID.
        public async Task<Episode?> GetEpisodeByIdAsync(EpisodeIdDTO episodeIdDTO)
        {
            try {
                // Custom validation to ensure the provided ID is valid.
                episodeIdDTO.ID.ThrowIfInvalidId("Episode Id");

                // Check whether the episode exist from the repository by ID.
                var episode = await _episodeRepository.GetEpisodeByIdAsync(episodeIdDTO);

                // Return null when the episode does not exist.
                return episode;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while checking if an episode exist by its id: {ex.Message}.");
            }
        }



        // Method signature for creating a new episode. Returns the Id of the created episode indicating success.
        public async Task<EpisodeIdDTO> CreateEpisodeAsync(Episode episode)
        {
            try {
                var result = await _episodeRepository.CreateEpisodeAsync(episode);

                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while creating an episode: {ex.Message}.");
            }
        }

        public async Task<bool> SetEpisodeStatusAsync(int episodeID, int doctorID)
        {
            try
            {
                var episode = await _episodeRepository.GetEpisodeByIdAsync(new EpisodeIdDTO { ID = episodeID }) ?? throw new InvalidOperationException($"Episode with ID {episodeID} not found.");

                if (episode.DoctorID != doctorID)
                {
                    throw new InvalidOperationException($"Doctor with ID {doctorID} cannot update episode with ID {episodeID}.");
                }

                var result = await _episodeRepository.SetEpisodeStatusAsync(episodeID);

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error while updating episode status: {ex.Message}");
            }
        }
    }
}
