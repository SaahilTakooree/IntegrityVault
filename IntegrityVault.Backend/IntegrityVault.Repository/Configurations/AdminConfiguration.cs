// Import the dependencies that is needed to create the configuraiton of the admin for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the admin entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the admin entity maps to the database.
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<Admin> entity)
        {
            // Maps the external admin to the database table name "Admins".
            entity.ToTable("Admins");

            // Configure one-to-one relationship with User table.
            entity.HasOne<Admin>() // Reference the Admin Entity to another entity.
                .WithOne() // Specifies that the other entity has one instance of the external provider.
                .HasForeignKey<Admin>(a => a.ID) // Set the foreign key which is the Admin's "ID" column
                .OnDelete(DeleteBehavior.Cascade); // Define when a user get deleted, the associated Admin is also deleted.
        }
    }
}