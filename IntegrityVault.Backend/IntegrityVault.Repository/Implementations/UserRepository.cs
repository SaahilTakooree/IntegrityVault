// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IUserRepository interface to implement the repository.
using IntegrityVault.Common.DTOs; // Import data transfer objects used in the repository for user creation.
using IntegrityVault.Common.Entities; // Import the entity classes representing the different types of users.
using IntegrityVault.Common.Enums; // Import the enums used to define user roles.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IUserRepository interface, with the DbContext injected for database access.
    public class UserRepository(IntegrityVaultDbContext _context) : IUserRepository
    {
        // Method to fetch all users from the database asynchronously.
        public async Task<IEnumerable<User>> GetAllUsersAsync(int? hospitalID = null)
        {
            try
            {
                // Define all variable
                List<User> admins;
                List<User> doctors;
                List<User> externalProviders;
                List<User> patients;
                List<User> superAdmins;


                // Check needs a user from a specific hospital.
                if (hospitalID.HasValue)
                {
                    admins = await _context.Admins
                        .Where(u => u.HospitalID == hospitalID)
                        .ToListAsync<User>();
                    doctors = await _context.Doctors
                        .Where(u => u.HospitalID == hospitalID)
                        .ToListAsync<User>();
                    externalProviders = await _context.ExternalProviders
                        .Where(u => u.HospitalID == hospitalID)
                        .ToListAsync<User>();
                    patients = await _context.Patients
                        .Where(u => u.HospitalID == hospitalID)
                        .ToListAsync<User>();
                }
                else
                {
                    admins = await _context.Admins.ToListAsync<User>();
                    doctors = await _context.Doctors.ToListAsync<User>();
                    externalProviders = await _context.ExternalProviders.ToListAsync<User>();
                    patients = await _context.Patients.ToListAsync<User>();
                }

                // Fetch all superuser as they don't belong to only one hospital from the Users table asynchronously and converting to a list.
                superAdmins = await _context.SuperAdmins.ToListAsync<User>();

                return doctors
                    .Concat(patients)
                    .Concat(admins)
                    .Concat(superAdmins)
                    .Concat(externalProviders);

            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                Console.WriteLine($"Error while retrieving all users {ex.Message}."); // Log the error message to the console.
                throw new InvalidOperationException($"Error retrieving users from the database.{ex.Message}"); // Throw a custom exception with the error message.
            }
        }


        // Method to fetch a single user by ID asynchronously.
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                // Finding the user by ID asynchronously, returning null if not found.
                User? user = await _context.Doctors.FirstOrDefaultAsync(u => u.ID == id);
                user ??= await _context.Patients.FirstOrDefaultAsync(u => u.ID == id);
                user ??= await _context.Admins.FirstOrDefaultAsync(u => u.ID == id);
                user ??= await _context.SuperAdmins.FirstOrDefaultAsync(u => u.ID == id);
                user ??= await _context.ExternalProviders.FirstOrDefaultAsync(u => u.ID == id);
                return user;
            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                {
                    Console.WriteLine($"Error while retrieving user by ID {id} {ex.Message}."); // Log the error message to the console.
                    throw new InvalidOperationException($"Error retrieving user with ID {id} from the database {ex.Message}"); // Throw a custom exception with the error message.
                }
            }
        }


        //  Method to create a new doctor in the database asynchronously.
        public async Task<bool> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO)
        {
            try
            {
                var doctor = new Doctor
                {
                    Username = createDoctorDTO.Username,
                    Email = createDoctorDTO.Email,
                    Password = createDoctorDTO.Password,
                    Role = UserRole.Doctor,
                    HospitalID = createDoctorDTO.HospitalID,
                    FirstName = createDoctorDTO.FirstName,
                    MiddleName = createDoctorDTO.MiddleName,
                    LastName = createDoctorDTO.LastName,
                    Specialty = createDoctorDTO.Specialty
                };

                // Save changes and return true if successful.
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during doctor creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a doctor {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new doctor {ex.Message}."); // Throw a custom exception for general errors during user creation.
            }
        }


        //  Method to create a new patient in the database asynchronously.
        public async Task<bool> CreatePatientAsync(CreatePatientDTO createPatientDTO)
        {
            try
            {
                var patient = new Patient
                {
                    Username = createPatientDTO.Username,
                    Email = createPatientDTO.Email,
                    Password = createPatientDTO.Password,
                    Role = UserRole.Patient,
                    HospitalID = createPatientDTO.HospitalID,
                    FirstName = createPatientDTO.FirstName,
                    MiddleName = createPatientDTO.MiddleName ?? "",
                    LastName = createPatientDTO.LastName,
                    DOB = createPatientDTO.DOB,
                    Gender = createPatientDTO.Gender
                };

                // Save changes and return true if successful.
                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during patient creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a patient {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new patient {ex.Message}."); // Throw a custom exception for general errors during user creation.
            }
        }


        //  Method to create a new admin in the database asynchronously.
        public async Task<bool> CreateAdminAsync(CreateAdminDTO createAdminDTO)
        {
            try
            {
                var admin = new Admin
                {
                    Username = createAdminDTO.Username,
                    Email = createAdminDTO.Email,
                    Password = createAdminDTO.Password,
                    Role = UserRole.Admin,
                    HospitalID = createAdminDTO.HospitalID
                };

                // Save changes and return true if successful.
                await _context.Admins.AddAsync(admin);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during admin creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating an admin {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new admin {ex.Message}."); // Throw a custom exception for general errors during user creation.
            }
        }


        //  Method to create a new external provider in the database asynchronously.
        public async Task<bool> CreateExternalProviderAsync(CreateExternalProviderDTO createExternalProviderDTO)
        {
            try
            {
                var externalprovider = new ExternalProvider
                {
                    Username = createExternalProviderDTO.Username,
                    Email = createExternalProviderDTO.Email,
                    Password = createExternalProviderDTO.Password,
                    Role = UserRole.ExternalProvider,
                    HospitalID = createExternalProviderDTO.HospitalID,
                    BelongsToID = createExternalProviderDTO.BelongsToID
                };

                // Save changes and return true if successful.
                await _context.ExternalProviders.AddAsync(externalprovider);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during external provider creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating an external provider {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new external provider {ex.Message}."); // Throw a custom exception for general errors during user creation.
            }
        }


        //  Method to create a new super admin in the database asynchronously.
        public async Task<bool> CreateSuperAdminAsync(CreateSuperAdminDTO createSuperAdminDTO)
        {
            try
            {
                var superadmin = new SuperAdmin
                {
                    Username = createSuperAdminDTO.Username,
                    Email = createSuperAdminDTO.Email,
                    Password = createSuperAdminDTO.Password,
                    Role = UserRole.SuperAdmin,
                    HospitalID = null
                };

                // Save changes and return true if successful.
                await _context.SuperAdmins.AddAsync(superadmin);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during super admin creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating an super admin {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new super admin {ex.Message}."); // Throw a custom exception for general errors during user creation.
            }
        }


        // Method to update a doctor record asynchronously.
        public async Task<bool> UpdateDoctorAsync(int id, UpdateDoctorDTO updateDoctorDTO)
        {
            try
            {
                // Fetch the Doctor from the Doctors table using EF Core TPT navigation.
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.ID == id) ?? throw new InvalidOperationException($"Doctor with ID {id} was not found.");

                // Apply the base user fields.
                ApplyBaseUserUpdates(doctor, updateDoctorDTO);

                // Apply Doctor-specific fields.
                if (updateDoctorDTO.FirstName is not null) doctor.FirstName = updateDoctorDTO.FirstName;
                if (updateDoctorDTO.MiddleName is not null) doctor.MiddleName = updateDoctorDTO.MiddleName;
                if (updateDoctorDTO.LastName is not null) doctor.LastName = updateDoctorDTO.LastName;
                if (updateDoctorDTO.Specialty.HasValue) doctor.Specialty = updateDoctorDTO.Specialty.Value;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error while updating doctor {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating doctor with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating doctor {id}: {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating doctor with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during doctor updating.
            }
        }


        // Method to update a patient record asynchronously.
        public async Task<bool> UpdatePatientAsync(int id, UpdatePatientDTO updatePatientDTO)
        {
            try
            {
                // Fetch the Patient from the Patients table using EF Core TPT navigation.
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.ID == id) ?? throw new InvalidOperationException($"Patient with ID {id} was not found."); 

                // Apply the base user fields.
                ApplyBaseUserUpdates(patient, updatePatientDTO);

                // Apply Patient-specific fields.
                if (updatePatientDTO.FirstName is not null) patient.FirstName = updatePatientDTO.FirstName;
                if (updatePatientDTO.MiddleName is not null) patient.MiddleName = updatePatientDTO.MiddleName;
                if (updatePatientDTO.LastName is not null) patient.LastName = updatePatientDTO.LastName;
                if (updatePatientDTO.DOB.HasValue) patient.DOB = updatePatientDTO.DOB.Value;
                if (updatePatientDTO.Gender.HasValue) patient.Gender = updatePatientDTO.Gender.Value;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while updating patient {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating patient with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating patient {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating patient with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during patient updating.
            }
        }


        // Method to update am admin record asynchronously.
        public async Task<bool> UpdateAdminAsync(int id, UpdateAdminDTO updateAdminDTO)
        {
            try
            {
                // Fetch the admin from the Admins table using EF Core TPT navigation.
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.ID == id) ?? throw new InvalidOperationException($"Admin with ID {id} was not found."); 

                // Apply the base user fields.
                ApplyBaseUserUpdates(admin, updateAdminDTO);


                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while updating admin {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating admin with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating admin {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating admin with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during admin updating.
            }
        }


        // Method to update an external providers record asynchronously.
        public async Task<bool> UpdateExternalProviderAsync(int id, UpdateExternalProviderDTO updateExternalProviderDTO)
        {
            try
            {
                // Fetch the external providers from the ExternalProviders table using EF Core TPT navigation.
                var externalProviders = await _context.ExternalProviders.FirstOrDefaultAsync(e => e.ID == id) ?? throw new InvalidOperationException($"External providers with ID {id} was not found.");

                // Apply the base user fields.
                ApplyBaseUserUpdates(externalProviders, updateExternalProviderDTO);

                // Apply ExternalProvider-specific field.
                if (updateExternalProviderDTO.BelongsToID.HasValue)
                    externalProviders.BelongsToID = updateExternalProviderDTO.BelongsToID.Value;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while updating external providers {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating external providers with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating external providers {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating external providers with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during external providers updating.
            }
        }


        // Method to update an super admins record asynchronously.
        public async Task<bool> UpdateSuperAdminAsync(int id, UpdateSuperAdminDTO updateSuperAdminDTO)
        {
            try
            {
                // Fetch the super admins from the SuperAdmins table using EF Core TPT navigation.
                var superadmins = await _context.SuperAdmins.FirstOrDefaultAsync(e => e.ID == id) ?? throw new InvalidOperationException($"Super admins with ID {id} was not found.");

                // Apply the base user fields.
                ApplyBaseUserUpdates(superadmins, updateSuperAdminDTO);


                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while updating super admins {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating super admins with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating super admins {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating super admins with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during super admin updating.
            }
        }


        // Method to delete a user asynchronously.
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                // Look up the user in the shared Users table.
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id) ?? throw new InvalidOperationException($"User with ID {id} was not found.");

                // Removing the base User entity is enough because all role-specific
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while deleting user {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while deleting user with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while deleting user {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error deleting user with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during user deletion.
            }
        }


        // Private helper to applie the shared based-user field form any UpdateUserDTO
        private static void ApplyBaseUserUpdates(User user, UpdateUserDTO updateUserDTO)
        {
            if (updateUserDTO.Username is not null) user.Username = updateUserDTO.Username;
            if (updateUserDTO.Email is not null) user.Email = updateUserDTO.Email;
            if (updateUserDTO.Password is not null) user.Password = updateUserDTO.Password;
            if (updateUserDTO.HospitalID.HasValue) user.HospitalID = updateUserDTO.HospitalID;
        }
    }
}