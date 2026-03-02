// Import the dependencies that is needed to create the configuraiton of the super admin for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the super admin entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the super admin entity maps to the database.
    public class SuperAdminConfiguration : IEntityTypeConfiguration<SuperAdmin>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<SuperAdmin> entity)
        {
            // Maps the external super admin to the database table name "Admins".
            entity.ToTable("SuperAdmins");

            // Configure one-to-one relationship with User table.
            entity.HasOne<SuperAdmin>() // Reference the SuperAdmin Entity to another entity.
                .WithOne() // Specifies that the other entity has one instance of the external provider.
                .HasForeignKey<SuperAdmin>(s => s.ID) // Set the foreign key which is the SuperAdmin's "ID" column
                .OnDelete(DeleteBehavior.Cascade); // Define when a user get deleted, the associated SuperAdmin is also deleted.
        }
    }
}