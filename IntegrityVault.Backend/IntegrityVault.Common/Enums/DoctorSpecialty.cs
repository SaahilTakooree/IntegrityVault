// Define the namespace for the enums types in the IntegrityVault project.
namespace IntegrityVault.Common.Enums
{
    // Enum representing the specialty of a doctor. Stored as a byte in the database.
    public enum DoctorSpecialty : byte
    {
        // Heart-related medicine specialty.
        Cardiology = 0,

        // Skin-related medicine specialty.
        Dermatology = 1,

        // General medicine for common illnesses.
        GeneralMedicine = 2,

        // Nervous system and brain specialty.
        Neurology = 3,

        // Child health and pediatric care speciality.
        Pediatrics = 4
    }
}
