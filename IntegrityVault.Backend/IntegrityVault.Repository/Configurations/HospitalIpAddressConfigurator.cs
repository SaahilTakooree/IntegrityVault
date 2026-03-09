// Import the dependencies that is needed to create the configuraiton of the hospital ip address for the model builder.
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Allow configuration of the entity types.
using IntegrityVault.Common.Entities; // Contains the hospital ip address entity class.
using Microsoft.EntityFrameworkCore; // Main EF core namespace.


// Define the namespace for the configuration in the IntegrityVault project.
namespace IntegrityVault.Repository.Configurations
{
    // Class to configure how the hospitalIpAddress entity maps to the database.
    public class HospitalIpAddressConfiguration : IEntityTypeConfiguration<HospitalIpAddress>
    {
        // Method to automatically called by the EF Core when the building the model.
        public void Configure(EntityTypeBuilder<HospitalIpAddress> entity)
        {
            // Maps the hospital ip address entity to the database table name "HospitalIpAddresses".
            entity.ToTable("HospitalIpAddresses");

            // Set the primary key.
            entity.HasKey(h => h.ID);

            // Configure IpAddress column.
            entity.Property(ip => ip.IpAddress)
                .IsRequired()
                .HasMaxLength(45); // Supports both IPv4 and IPv6.

            // Each IP address must be unique across the same hospital. However 2 hopital can have the same ip address.
            entity.HasIndex(ip => new { ip.HospitalID, ip.IpAddress })
                .IsUnique();

            // Many IpAddresses belong to one Hospital.
            entity.HasOne(ip => ip.Hospital)
                .WithMany(h => h.IpAddresses)
                .HasForeignKey(ip => ip.HospitalID)
                .OnDelete(DeleteBehavior.Cascade); // Delete IPs when Hospital is deleted.
        }
    }
}