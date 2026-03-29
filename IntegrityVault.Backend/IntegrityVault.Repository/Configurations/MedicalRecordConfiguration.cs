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


            // Configure the relationshop between MedicalRecord and Episode.
            entity.HasOne(m => m.Episode) // Medical Record belongs to one episode.
                .WithMany(e => e.Records) // Episode have many medical record.
                .HasForeignKey(m => m.EpisodeID) // "EpisodeID" is the foreign key on the MedicalRecord table.
                .OnDelete(DeleteBehavior.Restrict); // Prevents deletion of an Episode if any MedicalRecord is linked.


            // Configure the VisitDate property.
            entity.Property(m => m.VisitDate)
                .IsRequired() // Make the VisitDate column not null.
                .HasColumnType("date"); // Store as date only.


            // Configure the IPFS_CID property.
            entity.Property(m => m.IPFS_CID)
                .IsRequired() // Make the IPFS_CID column not null.
                .HasMaxLength(90); // Set the maximum length to 90 characters.
            entity.ToTable(t => {
                t.HasCheckConstraint("CK_Medical_Record_IPFS_CID_Length",
                    "LEN(IPFS_CID) >= 40"); // Ensures tha each length of each CID is equal to or more than 40 characters long.
            });


            // Configure the ContentHash property.
            // Stores the SHA-256 hash of the raw PDF bytes (64 hex characters).
            entity.Property(m => m.ContentHash)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false); // Hash is always ASCII hex — no need for Unicode storage.
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MedicalRecord_ContentHash_Length",
                    "LEN(ContentHash) = 64");
            });

            // Configure the VersionHash property.
            entity.Property(m => m.VersionHash)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MedicalRecord_VersionHash_Length",
                    "LEN(VersionHash) = 64");
            });


            // Configure the PreviousVersionHash property.
            entity.Property(m => m.PreviousVersionHash)
                .IsRequired(false)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MedicalRecord_PreviousVersionHash_Length",
                    "PreviousVersionHash IS NULL OR LEN(PreviousVersionHash) = 64");
            });


            // Configure the BlockchainTxHash property.
            entity.Property(m => m.BlockchainTxHash)
                .IsRequired(false)
                .HasMaxLength(66)
                .IsUnicode(false);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MedicalRecord_BlockchainTxHash_Length",
                    "BlockchainTxHash IS NULL OR LEN(BlockchainTxHash) = 66");
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
                .HasDefaultValueSql("GETUTCDATE()"); // Sets the default value of UpdatedAt to the current UTC date and time.
        }
    }
}