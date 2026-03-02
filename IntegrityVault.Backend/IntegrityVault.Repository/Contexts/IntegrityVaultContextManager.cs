using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IntegrityVault.Repository.Contexts
{
    public class IntegrityVaultContextFactory : IDesignTimeDbContextFactory<IntegrityVaultDbContext>
    {
        public IntegrityVaultDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<IntegrityVaultDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new IntegrityVaultDbContext(optionsBuilder.Options);
        }
    }
}
