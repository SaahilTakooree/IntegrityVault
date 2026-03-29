// Define the namespace for the enums types in the IntegrityVault project.
namespace IntegrityVault.Common.Enums
{
    // Enum representing how user access a medical record. Stored as a byte in the database.
    public enum AccessType : byte
    {
        // Represent a user creating a new medical record'.
        Create = 0,

        // Represent a user download a medical record.
        Download = 1,

        // Represent a user updating an existing medical record.
        Update = 2,

        // Represent a user verifying a medical record.
        Verify = 3,

        // Represent a user simply viewing a record without changes.
        View = 4
    }
}
