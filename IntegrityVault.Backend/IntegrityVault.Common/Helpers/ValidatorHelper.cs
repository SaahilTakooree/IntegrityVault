// Defines the Helpers namespace for the IntegrityVault system.
namespace IntegrityVault.Common.Helpers
{
    // Helper class to verify if passing in data is null or not.
    public static class Validations
    {
        // Method to checks if the given string is null, empty, or only whitespace
        public static void ThrowIfNullOrEmpty(this string? value, string label)
        {
            // Verifies if the value is either null, empty, or contains only white space characters.
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{label} cannot be null or empty."); // Throws an ArgumentException if the condition is met to indicating the parameter cannot be null or empty
            }
        }


        // Method to check if the given integer is valid.
        public static void ThrowIfInvalidId(this int value, string label)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{label} must be a positive integer and cannot be zero or negative.");
            }
        }


        // Method to check if a given object is null.
        public static void ThrowIfNull<T>(this T? obj, string label) where T : class
        {
            // Verify is the object is null.
            if (obj == null)
            {
                throw new KeyNotFoundException($"{label} not found or is null."); // Throws an ArgumentException if the condition is met to indicating the parameter cannot be null or empty.
            }
        }
    }
}