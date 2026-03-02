// Import the dependencies that is needed to create the configuraiton of the doctor for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the doctor entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the doctor entity maps to the database.
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<Doctor> entity)
        {
            // Maps the doctor entity to the database table name "Doctors".
            entity.ToTable("Doctors");

            // Configure the FirstName property.
            entity.Property(d => d.FirstName)
                .IsRequired() // Make the FirstName column not null.
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the MiddleName property.
            entity.Property(d => d.MiddleName)
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the LastName property.
            entity.Property(d => d.LastName)
                .IsRequired() // Make the LastName column not null.
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the Specialty property.
            entity.Property(d => d.Specialty)
                .IsRequired() // Make the Specialty column not null.
                .HasMaxLength(3); // Set the maximum length to 3 charactes.
            entity.ToTable(d => {
                d.HasCheckConstraint("Ck_Doctor_Specialty",
                    "[Specialty] IN (0, 1, 2, 3, 4)"); // Make sure that Specialty column can only take values 0, 1, 2, 3, or 4.
            });

            // Configure one-to-one relationship with User table.
            entity.HasOne<Doctor>() // Reference the Doctor Entity to another entity.
                .WithOne() // Specifies that the other entity has one instance of the doctor.
                .HasForeignKey<Doctor>(d => d.ID) // Set the foreign key which is the Doctor's "ID" column
                .OnDelete(DeleteBehavior.Cascade); // Define when a user get deleted, the associated Doctor is also deleted.
        }
    }
}