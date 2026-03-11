// Import dependencies.
using IntegrityVault.Repository.Contexts; // Import the context class for interacting with the database.
using IntegrityVault.Repository.Interfaces; // Import the IHospitalRepository interface to implement the repository.
using IntegrityVault.Common.DTOs; // Import data transfer objects used in the repository for hospital creation.
using IntegrityVault.Common.Entities; // Import the entity classes representing hospital.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database interaction.


// Declare the namespace for the repository implementations.
namespace IntegrityVault.Repository.Implementations
{
    // Implemente the IHospitalRepository interface, with the DbContext injected for database access.
    public class HospitalRepository(IntegrityVaultDbContext _context) : IHospitalRepository
    {
        // Method to fetch all hospitals from the database asynchronously.
        public async Task<IEnumerable<Hospital>> GetAllHospitalsAsync()
        {
            try
            {
                // Fetch all hospitals from the Hospitals table asynchronously and converting to a list.
                return await _context!.Hospitals
                    .Include(h => h.IpAddresses)
                    .ToListAsync();
            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                Console.WriteLine($"Error while retrieving all hospitals {ex.Message}."); // Log the error message to the console.
                throw new InvalidOperationException($"Error retrieving hospitals from the database.{ex.Message}"); // Throw a custom exception with the error message.
            }
        }


        // Method to fetch a single hospital by ID asynchronously.
        public async Task<Hospital?> GetHospitalByIdAsync(int id)
        {
            try
            {
                // Finding the Hospital by ID asynchronously, returning null if not found.
                return await _context!.Hospitals
                    .Include(h => h.IpAddresses)
                    .FirstOrDefaultAsync(h => h.ID == id);
            }
            catch (Exception ex) // Catch any general exceptions during data fetching.
            {
                {
                    Console.WriteLine($"Error while retrieving hospital by ID {id} {ex.Message}."); // Log the error message to the console.
                    throw new InvalidOperationException($"Error retrieving hospital with ID {id} from the database {ex.Message}"); // Throw a custom exception with the error message.
                }
            }
        }


        //  Method to create a new hospital in the database asynchronously.
        public async Task<bool> CreateHospitalAsync(CreateHospitalDTO createHospitalDTO)
        {
            try
            {
                var hospital = new Hospital
                {
                    Name = createHospitalDTO.Name,
                    WalletAddress = createHospitalDTO.WalletAddress,
                    IpAddresses = createHospitalDTO.IpAddresses // Map each IP string from the DTO into a HospitalIpAddress entity.
                        .Select(ip => new HospitalIpAddress { IpAddress = ip })
                        .ToList()
                };

                // Save changes and return true if successful.
                await _context.Hospitals.AddAsync(hospital);
                await _context.SaveChangesAsync();

                return true; // Return true to show success.
            }
            catch (DbUpdateException dbEx) // Catch database update exceptions specifically.
            {
                Console.WriteLine($"Database update error {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating the database during hospital creation {dbEx.Message}."); // Throw a custom exception for database update errors.
            }
            catch (Exception ex) // Catch any other general exceptions.
            {
                Console.WriteLine($"General error while creating a hospital {ex.Message}."); // Log a general error message.
                throw new InvalidOperationException($"Error while creating a new hospital {ex.Message}."); // Throw a custom exception for general errors during hospital creation.
            }
        }


        // Method to update an hospital record asynchronously.
        public async Task<bool> UpdateHospitalAsync(int id, UpdateHospitalDTO updateHospitalDTO)
        {
            try
            {
                // Fetch the hospital from the Hospital table using EF Core TPT navigation.
                var hospitals = await _context.Hospitals
                    .Include(h => h.IpAddresses)
                    .FirstOrDefaultAsync(h => h.ID == id)
                    ?? throw new InvalidOperationException($"Hospital with ID {id} was not found.");

                // Apply the base user fields.
                if (updateHospitalDTO.Name is not null)
                {
                    hospitals.Name = updateHospitalDTO.Name;
                }

                if (updateHospitalDTO.WalletAddress is not null)
                {
                    hospitals.WalletAddress = updateHospitalDTO.WalletAddress;
                }

                if (updateHospitalDTO.IpAddresses is not null)
                {
                    var current = hospitals.IpAddresses.Select(x => x.IpAddress).ToHashSet();
                    var desired = updateHospitalDTO.IpAddresses.ToHashSet();

                    var toAdd = desired.Except(current);
                    var toRemove = current.Except(desired);

                    foreach (var ip in toRemove)
                    {
                        var entry = hospitals.IpAddresses.First(x => x.IpAddress == ip);
                        _context.HospitalIpAddresses.Remove(entry);
                    }

                    foreach (var ip in toAdd)
                        hospitals.IpAddresses.Add(new HospitalIpAddress { HospitalID = id, IpAddress = ip });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while updating hospital {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while updating hospital with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while updating hospital {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error while updating hospital admins with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during hospital updating.
            }
        }


        // Check if an incoming IP is authorised for a specific hospital.
        public async Task<bool> IsIpAuthorisedAsync(int hospitalId, string ipAddress)
        {
            try
            {
                return await _context.HospitalIpAddresses
                    .AnyAsync(ip => ip.HospitalID == hospitalId && ip.IpAddress == ipAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while checking IP authorisation: {ex.Message}."); // Log the database checking error.
                throw new InvalidOperationException($"Error checking IP authorisation. {ex.Message}"); // Throw a custom exception for database checking errors.
            }
        }


        // Method to check if an id actually exists in the hospital table.
        public async Task<bool> ExistsAsync(int hospitalId)
        {
            return await _context.Hospitals.AnyAsync(h => h.ID == hospitalId);
        }


        // Method to delete a hospital asynchronously.
        public async Task<bool> DeleteHospitalAsync(int id)
        {
            try
            {
                // Look up the hospital in the shared Hospitals table.
                var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.ID == id) ?? throw new InvalidOperationException($"Hospital with ID {id} was not found.");

                // Removing the base Hospital entity is enough because all role-specific
                _context.Hospitals.Remove(hospital);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException) // Re-throw business-rule exceptions as-is.
            {
                throw;
            }
            catch (DbUpdateException dbEx) // Re-throw business-rule exceptions as-is.
            {
                Console.WriteLine($"Database update error while deleting hospital {id}: {dbEx.Message}."); // Log the database update error.
                throw new InvalidOperationException($"Error while deleting hospital with ID {id}. {dbEx.Message}"); // Throw a custom exception for database update errors.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while deleting hospital {id}: {ex.Message}.");  // Log a general error message.
                throw new InvalidOperationException($"Unexpected error deleting hospital with ID {id}. {ex.Message}"); // Throw a custom exception for general errors during hospital deletion.
            }  
        }
    }
}
