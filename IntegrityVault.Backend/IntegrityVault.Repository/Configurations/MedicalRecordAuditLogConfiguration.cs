// Import the dependencies that is needed to create the configuration of the audit log for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allows configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the audit logs entity class.
using Microsoft.EntityFrameworkCore; // Main EF Core namespace.


// Class to configure how the MedicalRecordAuditLog entity maps to the database.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the MedicalRecordAuditLog entity maps to the database.
    public class MedicalRecordAuditLogConfiguration : IEntityTypeConfiguration<MedicalRecordAuditLog>
    {
        public void Configure(EntityTypeBuilder<MedicalRecordAuditLog> entity)
        {
            // Maps the audit log entity to the database table name "MedicalRecordAuditLogs".
            entity.ToTable("MedicalRecordAuditLogs");

            // Set the primary key.
            entity.HasKey(m => m.ID);

            // Configure the relationship between MedicalRecordAuditLog and MedicalRecord.
            entity.HasOne(r => r.Record)
                .WithMany(m => m.AuditLogs)
                .HasForeignKey(r => r.RecordID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between MedicalRecordAuditLog and Doctor.
            entity.HasOne(a => a.UpdatedByDoctor)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByDoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the PreviousIPFS_CID property.
            entity.Property(a => a.PreviousIPFS_CID)
                .IsRequired() // Make the PreviousIPFS_CID column not null.
                .HasMaxLength(90); // Set the maximum length to 90 characters.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_AuditLog_PreviousIPFS_CID_Length",
                    "LEN(PreviousIPFS_CID) >= 40"); // Ensures the old CID is at least 40 characters long.
            });

            // Configure the NewIPFS_CID property.
            entity.Property(a => a.NewIPFS_CID)
                .IsRequired() // Make the NewIPFS_CID column not null.
                .HasMaxLength(90); // Set the maximum length to 90 characters.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_AuditLog_NewIPFS_CID_Length",
                    "LEN(NewIPFS_CID) >= 40"); // Ensures the new CID is at least 40 characters long.
            });

            // Ensure the old and new CIDs are never the same.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_AuditLog_CIDs_Must_Differ",
                    "PreviousIPFS_CID <> NewIPFS_CID");
            });

            // Configure the Version property.
            entity.Property(a => a.Version)
                .IsRequired(); // Make the Version column not null.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_AuditLog_Version_Positive",
                    "Version >= 1"); // Version numbers in the audit log must start at 1.
            });

            // Add a unique constraint on (RecordID, Version) so each version number is only used once per record.
            entity.HasIndex(a => new { a.RecordID, a.Version })
                .IsUnique();

            // Configure the UpdatedAt property.
            entity.Property(a => a.UpdatedAt)
                .IsRequired() // Make the UpdatedAt column not null.
                .HasColumnType("datetime2") // Use datetime2 for full precision.
                .HasDefaultValueSql("GETUTCDATE()"); // Default to the current UTC date and time.
        }
    }
}
