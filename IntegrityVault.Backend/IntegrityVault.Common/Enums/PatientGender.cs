// Define the namespace for the enums types in the IntegrityVault project.
namespace IntegrityVault.Common.Enums
{
    // Enum representing the gender of a patient. Stored as a byte in the database.
    public enum PatientGender : byte
    {
        // Represents a female patient.
        Female = 0,

        // Represents a male patient.
        Male = 1
    }
}
