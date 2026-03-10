// Import the dependencies that is needed to create the configuraiton of the patient for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the patient entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the patient entity maps to the database.
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<Patient> entity)
        {
            // Maps the patient entity to the database table name "Patients".
            entity.ToTable("Patients");

            // Configure the FirstName property.
            entity.Property(p => p.FirstName)
                .IsRequired() // Make the FirstName column not null.
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the MiddleName property.
            entity.Property(p => p.MiddleName)
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the LastName property.
            entity.Property(p => p.LastName)
                .IsRequired() // Make the LastName column not null.
                .HasMaxLength(100); // Set maximum length to the 100 characters.

            // Configure the DOB property.
            entity.Property(p => p.DOB)
                .IsRequired() // Make the DOB column not null.
                .HasColumnType("date"); // Make sure the date of birth is stored at date time.

            // Configure the Gender property.
            entity.Property(p => p.Gender)
                .IsRequired() // Make the Gender column not null.
                .HasMaxLength(3); // Set the maximum length to 3 charactes.
            entity.ToTable(t => {
                t.HasCheckConstraint("Ck_Patient_Gender",
                    "[Gender] IN (0, 1)"); // Make sure that Gender column can only take values 0, or 1.
            });

            // Configure one-to-one relationship with User table.
            entity.HasOne<Patient>() // Reference the Patient Entity to another entity.
                .WithOne() // Specifies that the other entity has one instance of the patient.
                .HasForeignKey<Patient>(p => p.ID) // Set the foreign key which is the Patient's "ID" column
                .OnDelete(DeleteBehavior.Cascade); // Define when a user get deleted, the associated Patient is also deleted.
        }
    }
}