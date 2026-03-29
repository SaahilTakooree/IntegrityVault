// Import dependencies.
using IntegrityVault.Common.Entities; // Entity models representing the domain objects.
using IntegrityVault.Common.Enums; // Emuns representing the acess types.


// Declaring the namespace where the mappers resides.
namespace IntegrityVault.Service.Mappers
{
    // Mapper class to convert data to a RecordAccessLog entity.
    internal static class ToRecordAccessLogMapper
        {
        // Method to map the relevant fields to a RecordAccessLog entity.
        internal static RecordAccessLog ToRecordAccessLogEntity(int recordID, int userID, byte accessType, DateTime currentTime)
        {
            // Return a new RecordAccessLog entity, with all necessary data populated.
            return new RecordAccessLog
            {
                RecordID = recordID,
                AccessedByUserID = userID,
                AccessType = (AccessType)Enum.ToObject(typeof(AccessType), accessType),
                Timestamp = currentTime
            };
        }
    }
}