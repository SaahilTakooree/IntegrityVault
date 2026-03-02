// Import the dependencies that is needed to create the configuraiton of the hospital for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the hospital entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the hospital entity maps to the database.
    public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<Hospital> entity)
        {
            // Maps the hospital entity to the database table name "Hospitals".
            entity.ToTable("Hospitals");

            // Set the primary key.
            entity.HasKey(h => h.ID);

            // Configure the Name property.
            entity.Property(h => h.Name)
                .IsRequired() // Make the Name column not null.
                .HasMaxLength(100); // Set maximum lenght to the 100 characters.

            // Configure the WalletAddress property. 
            entity.Property(h => h.WalletAddress)
                .IsRequired() // Make the WalletAddress column not null.
                .HasMaxLength(42);// Set the macimum length to the 43 characters.
            entity.ToTable(t => { 
                t.HasCheckConstraint("CK_Hospitals_WalletAddress_Length",
                    "LEN(WalletAddress) = 42"); // Ensures tha each length of each address is exactly 42 characters long.
            });
            entity.HasIndex(h => h.WalletAddress)
                .IsUnique(); // Ensure that each wallet address is unique.

            // Configure one-to-many relationship with User.
            entity.HasMany(h => h.Users) // One Hospital has many Users
                .WithOne(u => u.Hospital) // Each User has one Hospital.
                .HasForeignKey(u => u.HospitalID) // HospitalID is the foreign key in the Users Tabel
                .OnDelete(DeleteBehavior.SetNull); // If Hospital is deleted, Set User.HospitalID to NULL.
        }
    }
}