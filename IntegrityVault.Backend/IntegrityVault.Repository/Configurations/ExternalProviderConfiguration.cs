// Import the dependencies that is needed to create the configuraiton of the external provider for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the external provider entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the external provider entity maps to the database.
    public class ExternalProviderConfiguration : IEntityTypeConfiguration<ExternalProvider>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<ExternalProvider> entity)
        {
            // Maps the external provider entity to the database table name "ExternalProviders".
            entity.ToTable("ExternalProviders");

            // Configure one-to-one relationship with User table.
            entity.HasOne<ExternalProvider>() // Reference the ExternalProvider Entity to another entity.
                .WithOne() // Specifies that the other entity has one instance of the external provider.
                .HasForeignKey<ExternalProvider>(e => e.ID) // Set the foreign key which is the ExternalProvider's "ID" column
                .OnDelete(DeleteBehavior.Cascade); // Define when a user get deleted, the associated ExternalProvider is also deleted.
        }
    }
}