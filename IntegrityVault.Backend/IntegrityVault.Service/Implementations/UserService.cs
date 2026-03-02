// Import dependencies needed.
using IntegrityVault.Repository.Interfaces; // Import the interface for the user repository.
using IntegrityVault.Service.Interfaces; // Import the interface for the user service.
using IntegrityVault.Common.Entities; // Import the entity class for User.
using IntegrityVault.Common.Helpers; // Import helper functions.
using IntegrityVault.Common.DTOs; // Import the data transfer objects (DTOs) used in the service layer.
using IntegrityVault.Common.Enums; // Import the emnus for the UserRole

// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the UserService class and injecting the IUserRepository dependency.
    public class UserService(IUserRepository _userRepository) : IUserService
    {
        // Method to return all users mapped to their role-specific DTOs.
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                // Get all the user from the repository.
                var users = await _userRepository.GetAllUsersAsync();

                // Map each User entity to the most specific DTO avaliable for that role.
                return users.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching all users: {ex.Message}.");
            }
        }

        // Method to fetch a user by their ID asynchronously.
        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("User Id");

                // Fetch the user from the repository by ID.
                var user = await _userRepository.GetUserByIdAsync(id);

                // Return null when the user does not exist.
                if (user is null) return null;

                return MapToDTO(user);
            }
            catch (Exception ex) // Catching any general exceptions.
            {
                throw new InvalidOperationException($"Error fetching user by ID: {ex.Message}."); // Wrapping the exception and throwing it with a custom message.
            }
        }


        // Method to create a doctor asynchronously.
        public async Task<bool> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO)
        {
            try
            {
                // Check if the email already exists.
                if (await DoesEmailExist(createDoctorDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createDoctorDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (await DoesUsernameExist(createDoctorDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{createDoctorDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                createDoctorDTO.Password = HashHelper.Hash(createDoctorDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.


                // Create the doctor.
                return await _userRepository.CreateDoctorAsync(createDoctorDTO); // Create the doctor in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"Doctor creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for doctor creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during doctor creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to create a patient asynchronously.
        public async Task<bool> CreatePatientAsync(CreatePatientDTO createPatientDTO)
        {
            try
            {
                // Check if the email already exists.
                if (await DoesEmailExist(createPatientDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createPatientDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (await DoesUsernameExist(createPatientDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{createPatientDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                createPatientDTO.Password = HashHelper.Hash(createPatientDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.


                // Create the patient.
                return await _userRepository.CreatePatientAsync(createPatientDTO); // Create the patient in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"Patient creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for patient creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during patient creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to create an admin asynchronously.
        public async Task<bool> CreateAdminAsync(CreateAdminDTO createAdminDTO)
        {
            try
            {
                // Check if the email already exists.
                if (await DoesEmailExist(createAdminDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createAdminDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (await DoesUsernameExist(createAdminDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{createAdminDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                createAdminDTO.Password = HashHelper.Hash(createAdminDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.


                // Create the admin.
                return await _userRepository.CreateAdminAsync(createAdminDTO); // Create the admin in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"Admin creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for admin creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during admin creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to create an external provider asynchronously.
        public async Task<bool> CreateExternalProviderAsync(CreateExternalProviderDTO createExternalProviderDTO)
        {
            try
            {
                // Check if the email already exists.
                if (await DoesEmailExist(createExternalProviderDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createExternalProviderDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (await DoesUsernameExist(createExternalProviderDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{createExternalProviderDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                createExternalProviderDTO.Password = HashHelper.Hash(createExternalProviderDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.


                // Create the external provider.
                return await _userRepository.CreateExternalProviderAsync(createExternalProviderDTO); // Create the external provider in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"External provider creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for external provider creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during external provider creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to create a super admin asynchronously.
        public async Task<bool> CreateSuperAdminAsync(CreateSuperAdminDTO createSuperAdminDTO)
        {
            try
            {
                // Check if the email already exists.
                if (await DoesEmailExist(createSuperAdminDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{createSuperAdminDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (await DoesUsernameExist(createSuperAdminDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{createSuperAdminDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                createSuperAdminDTO.Password = HashHelper.Hash(createSuperAdminDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.


                // Create the super admin.
                return await _userRepository.CreateSuperAdminAsync(createSuperAdminDTO); // Create the super admin in the repository and returning the result.
            }
            catch (InvalidOperationException ex) // Catch InvalidOperationException separately to provide specific error messages.
            {
                throw new InvalidOperationException($"Super admin creation failed: {ex.Message}."); // Wrap the exception and throwing it with a custom message for super admin creation failure.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                throw new InvalidOperationException($"Error during super admin creation: {ex.Message},"); // Wrap  the exception and throwing it with a custom message for general errors.
            }
        }


        // Method to update a doctor repository.
        public async Task<bool> UpdateDoctorAsync(int id, UpdateDoctorDTO updateDoctorDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Doctor Id");

                // Check if the email already exists.
                if (updateDoctorDTO.Email is not null && await DoesEmailExist(updateDoctorDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updateDoctorDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (updateDoctorDTO.Username is not null && await DoesUsernameExist(updateDoctorDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{updateDoctorDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                if (updateDoctorDTO.Password is not null)
                {
                    updateDoctorDTO.Password = HashHelper.Hash(updateDoctorDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.
                }

                return await _userRepository.UpdateDoctorAsync(id, updateDoctorDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Doctor update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during doctor update: {ex.Message}.");
            }
        }


        // Method to update a patient repository.
        public async Task<bool> UpdatePatientAsync(int id, UpdatePatientDTO updatePatientDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Patient Id");

                // Check if the email already exists.
                if (updatePatientDTO.Email is not null && await DoesEmailExist(updatePatientDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updatePatientDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (updatePatientDTO.Username is not null && await DoesUsernameExist(updatePatientDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{updatePatientDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                if (updatePatientDTO.Password is not null)
                {
                    updatePatientDTO.Password = HashHelper.Hash(updatePatientDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.
                }

                return await _userRepository.UpdatePatientAsync(id, updatePatientDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Patient update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during patient update: {ex.Message}.");
            }
        }


        // Method to update a admin repository.
        public async Task<bool> UpdateAdminAsync(int id, UpdateAdminDTO updateAdminDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("Admin Id");

                // Check if the email already exists.
                if (updateAdminDTO.Email is not null && await DoesEmailExist(updateAdminDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updateAdminDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (updateAdminDTO.Username is not null && await DoesUsernameExist(updateAdminDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{updateAdminDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                if (updateAdminDTO.Password is not null)
                {
                    updateAdminDTO.Password = HashHelper.Hash(updateAdminDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.
                }

                return await _userRepository.UpdateAdminAsync(id, updateAdminDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Admin update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during admin update: {ex.Message}.");
            }
        }


        // Method to update a external providers repository.
        public async Task<bool> UpdateExternalProviderAsync(int id, UpdateExternalProviderDTO updateExternalProvidersDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("ExternalProviders Id");

                // Check if the email already exists.
                if (updateExternalProvidersDTO.Email is not null && await DoesEmailExist(updateExternalProvidersDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updateExternalProvidersDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (updateExternalProvidersDTO.Username is not null && await DoesUsernameExist(updateExternalProvidersDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{updateExternalProvidersDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                if (updateExternalProvidersDTO.Password is not null)
                {
                    updateExternalProvidersDTO.Password = HashHelper.Hash(updateExternalProvidersDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.
                }

                return await _userRepository.UpdateExternalProviderAsync(id, updateExternalProvidersDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"External providers update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during external providers update: {ex.Message}.");
            }
        }


        // Method to update a super admin repository.
        public async Task<bool> UpdateSuperAdminAsync(int id, UpdateSuperAdminDTO updateSuperAdminDTO)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("SuperAdmin Id");

                // Check if the email already exists.
                if (updateSuperAdminDTO.Email is not null && await DoesEmailExist(updateSuperAdminDTO.Email) != null) // Checking if a user with the same email already exists.
                {
                    throw new InvalidOperationException($"A user with the email '{updateSuperAdminDTO.Email}' already exists."); // Throwing an error if the email is already taken.
                }

                // Check if the username already exists.
                if (updateSuperAdminDTO.Username is not null && await DoesUsernameExist(updateSuperAdminDTO.Username) != null) // Checking if a user with the same username already exists.
                {
                    throw new InvalidOperationException($"A user with the username '{updateSuperAdminDTO.Username}' already exists."); // Throwing an error if the username is already taken.
                }

                // Hash the password before storing it.
                if (updateSuperAdminDTO.Password is not null)
                {
                    updateSuperAdminDTO.Password = HashHelper.Hash(updateSuperAdminDTO.Password) ?? throw new InvalidOperationException("Password hashing failed."); // Hashing the password using a helper method and ensuring it is not null.
                }

                return await _userRepository.UpdateSuperAdminAsync(id, updateSuperAdminDTO);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Super admin update failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during super admin update: {ex.Message}.");
            }
        }


        // Method to delete a user from repository.
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                // Custom validation to ensure the provided ID is valid.
                id.ThrowIfInvalidId("User Id");

                return await _userRepository.DeleteUserAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"User delete failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during user deletion: {ex.Message}.");
            }
        }


        // Private method to map the each user to its most appropriate DTO.
        private static UserDTO MapToDTO(User user)
        {
            return user.Role switch
            {
                UserRole.Doctor => new DoctorDTO
                {
                    ID = user.ID,
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password,
                    Role = user.Role,
                    JoinDate = user.JoinDate,
                    HospitalID = user.HospitalID,
                    FirstName = (user as Doctor)!.FirstName,
                    MiddleName = (user as Doctor)?.MiddleName,
                    LastName = (user as Doctor)!.LastName,
                    Specialty = (user as Doctor)!.Specialty
                },
                UserRole.Patient => new PatientDTO
                {
                    ID = user.ID,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    JoinDate = user.JoinDate,
                    HospitalID = user.HospitalID,
                    FirstName = (user as Patient)!.FirstName,
                    MiddleName = (user as Patient)?.MiddleName,
                    LastName = (user as Patient)!.LastName,
                    DOB = (user as Patient)!.DOB,
                    Gender = (user as Patient)!.Gender
                },
                UserRole.Admin => new AdminDTO
                {
                    ID = user.ID,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    JoinDate = user.JoinDate
                },
                UserRole.ExternalProvider => new ExternalProviderDTO
                {
                    ID = user.ID,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    JoinDate = user.JoinDate
                },
                // Fallback for any unhandled roles.
                _ => new UserDTO
                {
                    ID = user.ID,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    JoinDate = user.JoinDate,
                    HospitalID = user.HospitalID
                }
            };
        }


        // Private method to check if an email already exists in the database.
        private async Task<User?> DoesEmailExist(string email)
        {
            try
            {
                // Fetch all users from the repository.
                var allUsers = await _userRepository!.GetAllUsersAsync();

                // Search for the user with the matching email and returning the first match or null.
                return allUsers.FirstOrDefault(u => u.Email == email);
            }
            catch (Exception ex) // Catch any exceptions that may occur during the check.
            {
                throw new InvalidOperationException($"Error checking if email exists: {ex.Message}."); // Wrap the exception and throwing it with a custom message for email existence check failure.
            }
        }


        // Private method to check if a username already exists in the database.
        private async Task<User?> DoesUsernameExist(string username)
        {
            try
            {
                // Fetch all users from the repository.
                var allUsers = await _userRepository!.GetAllUsersAsync();

                // Search for the user with the matching username and returning the first match or null.
                return allUsers.FirstOrDefault(u => u.Username == username);            }
            catch (Exception ex) // Catching any exceptions that may occur during the check.
            {
                throw new InvalidOperationException($"Error checking if username exists: {ex.Message}"); // Wrapping the exception and throwing it with a custom message for username existence check failure.
            }
        }
    }
}