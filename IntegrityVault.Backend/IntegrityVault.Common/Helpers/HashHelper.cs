// Import the BCrypt.Net namespace for hashing and verifying string.
using BCrypt.Net;


// Defines the Helpers namespace for the IntegrityVault system.
namespace IntegrityVault.Common.Helpers
{
    // Helper class for hashing and verifying data.
    public static class HashHelper
    {
        // Static method to hash a string.
        public static string? Hash(string input)
        {
            try
            {
                // Hash the input using BCrypt and return the result
                return BCrypt.Net.BCrypt.HashPassword(input);
            }
            catch (Exception ex) // Catch any exception that may occur during hashing.
            {
                Console.WriteLine($"An error occurred while hashing: {ex.Message}"); // Log the error message.
                return null; // Return null if hashing fails.
            }
        }


        // Static method to verify if hash string matches a store hash string.
        public static bool VerifyInput (string hashInput, string input )
        {
            try
            {
                // Verify if the input matches the stored hash using BCrypt
                return BCrypt.Net.BCrypt.Verify(input, hashInput); // Return true if match, otherwise return false.
            }
            catch (Exception ex) // Catch any exception that may occur during verification.
            {
                Console.WriteLine($"An error occurred while verifying hash: {ex.Message}"); // Log the error
                return false; // Return false if verification fails.
            }
        }
    }
}
