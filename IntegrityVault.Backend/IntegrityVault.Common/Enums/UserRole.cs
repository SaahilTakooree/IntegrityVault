// Define the namespace for the enums types in the IntegrityVault project.
namespace IntegrityVault.Common.Enums
{
    // Enum representing the role of a user. Stored as a byte in the database.
    public enum UserRole : byte
    {
        // Represents an admin user.
        Admin = 0,

        // Represents a doctor user.
        Doctor = 1,

        // Represents an external provider user.
        ExternalProvider = 2,

        // Represents a patient user.
        Patient = 3,

        // Represent a super admin user.
        SuperAdmin = 4
    }
}
