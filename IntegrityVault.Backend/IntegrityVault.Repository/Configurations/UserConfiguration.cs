// Import the dependencies that is needed to create the configuraiton of the user for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the user entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the user entity maps to the database.
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<User> entity)
        {
            // Maps the user entity to the database table name "Users".
            entity.ToTable("Users");

            // Set the primary key.
            entity.HasKey(u => u.ID);

            // Configure the Username property.
            entity.Property(u => u.Username)
                .IsRequired() // Make the Username column not null.
                .HasMaxLength(100); // Set maximum length to the 100 characters.
            entity.HasIndex(u => u.Username)
                .IsUnique(); // Make sure the username is unique.

            // Configure the Email property.
            entity.Property(u => u.Email)
                .IsRequired() // Make the Email column not null.
                .HasMaxLength(255); // Set the maximum length to 255 characters.
            entity.HasIndex(u => u.Email)
                .IsUnique(); // Make sure the email is unique.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Users_Email_Format",
                    "Email LIKE '%_@__%.__%'"); // Check to ensure the email contains "@" and ".".
            });

            // Configure the Password property.
            entity.Property(u => u.Password)
                .IsRequired() // Make the Password column not null.
                .HasMaxLength(255); // Set the maximum length to 255 characters.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Users_Password_Length",
                    "LEN(Password) >= 7"); // Ensures tha each length of each address is equal to or more than 7 characters long.
            });

            // Configure the Role property.
            entity.Property(u => u.Role)
                .IsRequired() // Make the Role column not null.
                .HasMaxLength(3); // Set the maximum length to 3 charactes.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("Ck_User_Role",
                    "[Role] IN (0, 1, 2, 3, 4)"); // Make sure that Role column can only take values 0, 1, 2, 3, or 4.
            });

            // Configure the JoinDate property.
            entity.Property(u => u.JoinDate)
                .IsRequired() // Make the JoinDate column not null.
                .HasColumnType("date") // Set the column type to data.
                .HasDefaultValueSql("GETUTCDATE()"); // Sets the default value of JoinDate to the current UTC date and time.

            // Configure the HospitalID property.
            entity.Property(u => u.HospitalID)
                .IsRequired(false) // Make the HospitalID column nullable.
                .HasAnnotation("HospitalID", "This property must be set for Patients, Doctors, and Admins");
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_User_HospitalID_Required",
                    "(Role IN (0, 1, 2, 3) AND HospitalID IS NOT NULL) OR (Role IN (4) AND HospitalID IS NULL)"); // Remove the misplaced comma
            });
        }
    }
}
