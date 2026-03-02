// Import the dependencies that is needed to create the configuraiton of the record accesslog for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the record access log entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the record access log entity maps to the database.
    public class RecordAccessLogConfiguration : IEntityTypeConfiguration<RecordAccessLog>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<RecordAccessLog> entity)
        {
            // Maps the record access log entity to the database table name "RecordAccessLogs".
            entity.ToTable("RecordAccessLogs");

            // Set the primary key.
            entity.HasKey(m => m.ID);

            // Configure the relationshop between RecordAccessLog and Record.
            entity.HasOne(r => r.Record) // Record access log has one record.
                .WithMany(m => m.AccessLogs) // Record have many record access log.
                .HasForeignKey(r => r.RecordID) // "RecordID" is the foreign key on the RecordAccessLog table.
                .OnDelete(DeleteBehavior.Cascade); // Delete access log if medical record is deleted is linked.

            // Configure the relationshop between RecordAccessLog and AccessByUser.
            entity.HasOne(r => r.AccessedBy) // Record access log is be access by one user.
                .WithMany() // Many User can access many record access log.
                .HasForeignKey(m => m.AccessedByUserID) // "AccessedByUserID" is the foreign key on the RecordAccessLog table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of User if any RecordAccessLog is linked.

            // Configure the AccessType property.
            entity.Property(r => r.AccessType)
                .IsRequired() // Make the AccessType column not null.
                .HasMaxLength(3); // Set the maximum length to 3 charactes.
            entity.ToTable(t => {
                t.HasCheckConstraint("Ck_RecordAccessLog_AccessType",
                    "[AccessType] IN (0, 1)"); // Make sure that AccessType column can only take values 0, or 1.
            });

            // Configure the CreatedAt property.
            entity.Property(m => m.Timestamp)
                .IsRequired() // Make the Timestamp column not null.
                .HasColumnType("date") // Set the column type to data.
                .HasDefaultValueSql("GETUTCDATE()"); // Sets the default value of Timestamp to the current UTC date and time.
        }
    }
}