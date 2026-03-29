// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IRecordAccessLogRepository interface to implement the repository.
using IntegrityVault.Common.Entities; // Import the entity classes representing RecordAccessLog.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IRecordAccessLogRepository interface, with the DbContext injected for database access.
    public class RecordAccessLogRepository(IntegrityVaultDbContext _context) : IRecordAccessLogRepository
    {
        // Method to add a record access log to the database.
        public Task<bool> CreateRecordAccessLogAsync(RecordAccessLog recordAccessLog)
        {
            try
            {
                // Save changes and return true if successful.
                _context.RecordAccessLogs.Add(recordAccessLog);

                return Task.FromResult(true); // Return the true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during the record access log creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a record access log {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new record access log {ex.Message}."); // Throw a custom exception for general errors during record access log creation.
            }
        }
    }
}
