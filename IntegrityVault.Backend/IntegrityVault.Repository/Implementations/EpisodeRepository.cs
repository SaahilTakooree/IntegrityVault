// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IEpisodeRepository interface to implement the repository.
using IntegrityVault.Common.DTOs; // Import data transfer objects used in the repository for episode creation.
using IntegrityVault.Common.Entities; // Import the entity classes representing episode.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IEpisodeRepository interface, with the DbContext injected for database access.
    public class EpisodeRepository(IntegrityVaultDbContext _context) : IEpisodeRepository
    {

        // Method to check if an episode exist by ID asynchronously.
        public async Task<Episode?> GetEpisodeByIdAsync(EpisodeIdDTO episodeIdDTO)
        {
            try
            {
                // Find an episode by its ID asynchronously, returning null if not found.
                return await _context!.Episodes.FirstOrDefaultAsync(e => e.ID == episodeIdDTO.ID);
            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                {
                    Console.WriteLine($"Error while retrieving an episode by ID {episodeIdDTO.ID} {ex.Message}."); // Log the error message to the console.
                    throw new InvalidOperationException($"Error retrieving an episode with ID {episodeIdDTO.ID} from the database {ex.Message}"); // Throw a custom exception with the error message.
                }
            }
        }


        //  Method to create a new episode in the database.
        public Task<EpisodeIdDTO> CreateEpisodeAsync(Episode episode)
        {
            try
            {
                // Save changes and return true if successful.
                _context.Episodes.Add(episode);

                return Task.FromResult (new EpisodeIdDTO { ID = episode.ID }); // Return the ID to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during the episode creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while updating a hospital {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while updating a new episode {ex.Message}."); // Throw a custom exception for general errors during episode update.
            }
        }


        //  Method to toggle the isActive status of an episode.
        public async Task<bool> SetEpisodeStatusAsync(int episodeID)
        {
            try
            {
                var episode = await GetEpisodeByIdAsync(new EpisodeIdDTO { ID = episodeID}) ?? throw new InvalidOperationException($"Episode with ID {episodeID} not found.");

                // Toggle IsActive.
                episode.IsActive = !episode.IsActive;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database active status {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error updating the database active status {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Errorupdating the database active status {ex.Message}."); // Throw a custom exception for general errors during episode updating.
            }
        }


        // Method to check if an epsiode is active of not.
        public async Task<bool> IsEpisodeActiveAsync(int episodeId)
        {
            try
            {
                var episode = await _context.Episodes
                    .Where(e => e.ID == episodeId)
                    .Select(e => new { e.IsActive })
                    .FirstOrDefaultAsync();

                return episode == null ? throw new InvalidOperationException($"Episode with ID {episodeId} not found.") : episode.IsActive;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking IsActive for episode {episodeId}: {ex.Message}");
                throw new InvalidOperationException($"Error retrieving episode status: {ex.Message}");
            }
        }
    }
}