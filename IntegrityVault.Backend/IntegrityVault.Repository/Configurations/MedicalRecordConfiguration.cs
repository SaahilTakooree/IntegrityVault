// Import the dependencies that is needed to create the configuraiton of the medical record for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the medical record entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the medical record entity maps to the database.
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<MedicalRecord> entity)
        {
            // Maps the medical record entity to the database table name "MedicalRecords".
            entity.ToTable("MedicalRecords");

            // Set the primary key.
            entity.HasKey(m => m.ID);

            // Configure the relationshop between MedicalRecord and Patient.
            entity.HasOne(m => m.Patient) // Medical Record has one patient.
                .WithMany(p => p.MedicalRecords) // Patient have many medical record.
                .HasForeignKey(m => m.PatientID) // "PatientID" is the foreign key on the MedicalRecord table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of Patient if any MedicalRecord is linked.

            // Configure the relationshop between MedicalRecord and Doctor.
            entity.HasOne(m => m.Doctor) // Medical record is written by one doctor.
                .WithMany(d => d.CreatedRecords) // Doctor can create many medical record.
                .HasForeignKey(m => m.DoctorID) // "DoctorID" is the foreign key on the MedicalRecord table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of Doctor if any MedicalRecord is linked.

            // Configure the IPFS_CID property.
            entity.Property(m => m.IPFS_CID)
                .IsRequired() // Make the IPFS_CID column not null.
                .HasMaxLength(90); // Set the maximum length to 90 characters.
            entity.ToTable(t => {
                t.HasCheckConstraint("CK_Medical_Record_IPFS_CID_Length",
                    "LEN(IPFS_CID) >= 40"); // Ensures tha each length of each CID is equal to or more than 40 characters long.
            });

            //Configure the CurrentVersion Property.
            entity.Property(m => m.CurrentVersion)
                .IsRequired()
                .HasDefaultValue(0); // Default to 0.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MedicalRecord_CurrentVersion_NonNegative",
                    "CurrentVersion >= 0");
            });

            // Configure the CreatedAt property.
            entity.Property(m => m.CreatedAt)
                .IsRequired() // Make the CreatedAt column not null.
                .HasColumnType("datetime2") // Set the column type to data.
                .HasDefaultValueSql("GETUTCDATE()"); // Sets the default value of CreatedAt to the current UTC date and time.

            // Configure the UpdatedAt property.
            entity.Property(m => m.UpdatedAt)
                .IsRequired() // Make the UpdatedAt column not null.
                .HasColumnType("datetime2") // Set the column type to data.
                .HasDefaultValueSql("GETUTCDATE()"); // Sets the default value of CreatedAt to the current UTC date and time.
        }
    }
}