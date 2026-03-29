// Import the dependencies that is needed to create the configuraiton of the episode for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the episode entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the episode entity maps to the database.
    public class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<Episode> entity)
        {
            // Maps the episode entity to the database table name "Episodes".
            entity.ToTable("Episodes");

            // Set the primary key.
            entity.HasKey(e => e.ID);

            // Configure the relationship between Episode and Patient.
            entity.HasOne(e => e.Patient) // Episode belongs to one patient.
                .WithMany(p => p.Episodes) // Patient can have many episodes.
                .HasForeignKey(e => e.PatientID) // "PatientID" is the foreign key on the Episodes table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of a Patient if any Episode is linked.

            // Configure the relationship between Episode and Doctor.
            entity.HasOne(e => e.Doctor) // Episode is managed by one doctor.
                .WithMany(d => d.Episodes) // Doctor can manage many episodes.
                .HasForeignKey(e => e.DoctorID) // "DoctorID" is the foreign key on the Episodes table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of a Doctor if any Episode is linked.

            // Configure the Specialty property.
            entity.Property(e => e.Specialty)
                .IsRequired()
                .HasMaxLength(3);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Episode_Specialty",
                    "[Specialty] IN (0, 1, 2, 3, 4)"); // Must match valid DoctorSpecialty enum values.
            });

            // Configure the Title property.
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(120); // Short administrative label for the clinical issue.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Episode_Title_MinLength",
                    "LEN(Title) >= 3"); // Title must be at least 3 characters.
            });

            // Configure the IsActive property.
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true); // Episodes are active by default.

            // Configure the CreatedAt property.
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()"); // Default to current UTC date and time.
        }
    }
}