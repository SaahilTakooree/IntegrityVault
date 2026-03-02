// Define the namespace for the enums types in the IntegrityVault project.
namespace IntegrityVault.Common.Enums
{
    // Enum representing how user access a medical record. Stored as a byte in the database.
    public enum AccessType : byte
    {
        // Represent a user download a medical record.
        Download = 0,

        // Represent a user verifying a medical record.
        Verify = 1
    }
}
