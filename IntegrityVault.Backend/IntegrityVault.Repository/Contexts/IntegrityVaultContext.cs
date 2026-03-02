using Microsoft.EntityFrameworkCore;
using IntegrityVault.Common.Entities;
using IntegrityVault.Repository.Configurations;

namespace IntegrityVault.Repository.Contexts
{
    public class IntegrityVaultDbContext(DbContextOptions<IntegrityVaultDbContext> options) : DbContext(options)
    {
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<ExternalProvider> ExternalProviders { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<RecordAccessLog> RecordAccessLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new HospitalConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PatientConfiguration());
            modelBuilder.ApplyConfiguration(new DoctorConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalProviderConfiguration());
            modelBuilder.ApplyConfiguration(new AdminConfiguration());
            modelBuilder.ApplyConfiguration(new SuperAdminConfiguration());
            modelBuilder.ApplyConfiguration(new MedicalRecordConfiguration());
            modelBuilder.ApplyConfiguration(new RecordAccessLogConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
