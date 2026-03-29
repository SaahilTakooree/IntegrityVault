// Import dependencies.
using IntegrityVault.Common.Entities; // Imported to allow access to the RecordAccessLog entity.


// Declare the namespace for the repository interfaces.
namespace IntegrityVault.Repository.Interfaces
{
    // Define the IRecordAccessLogsRepository interface that represents the contract for record access logs related database operations.
    public interface IRecordAccessLogRepository
    {
        Task<bool> CreateRecordAccessLogAsync(RecordAccessLog recordAccessLog); // Method to create a medical record. return ture if it was successful.
    }
}
