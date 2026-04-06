using IntegrityVault.Repository.Contexts;


namespace IntegrityVault.Tests.Repository
{
    public class HospitalRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly HospitalRepository _repository;

        public HospitalRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new HospitalRepository(_context);
        }

        #region Helpers


        private static Hospital BuildTestHospital(int id = 101, string name = "Princeton-Plainsboro") => new()
        {
            ID = id,
            Name = name,
            WalletAddress = "0xHospitalWallet",
            EncryptedPrivateKey = [1, 2, 3],
            IpAddresses =
            [
                new() { IpAddress = "192.168.1.1" }
            ]
        };

        #endregion



        [Fact]
        public async Task GetAllHospitalsAsync_ShouldIncludeIpAddresses()
        {
            // Arrange.
            var hospital = BuildTestHospital();
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetAllHospitalsAsync();

            // Assert.
            Assert.NotEmpty(result);
            Assert.Single(result.First().IpAddresses);
            Assert.Equal("192.168.1.1", result.First().IpAddresses.First().IpAddress);
        }


        [Fact]
        public async Task CreateHospitalAsync_ShouldMapDtoCorrectly()
        {
            // Arrange.
            var dto = new CreateHospitalDTO
            {
                Name = "St. Sebastian",
                WalletAddress = "0xNewWallet",
                PrivateKey = "0xCFBFA67EC18F50DEDC25370F7416E3AFB57EBA4A9443126E22AD5B5D389B5A36BAD23BD3ED123121501A5E4D89E28EE88A24C9A315E9087A926D0C07783F4C896A6DFA278B098531A086833ED719CF5250BC28CD1C46BA66725F0012E8F1",
                IpAddresses = ["10.0.0.1", "10.0.0.2"]
            };
            byte[] key = [9, 9, 9];

            // Act.
            var hospitalId = await _repository.CreateHospitalAsync(dto, key);

            // Assert.
            var hospital = await _context.Hospitals.Include(h => h.IpAddresses).FirstOrDefaultAsync(h => h.ID == hospitalId);
            Assert.NotNull(hospital);
            Assert.Equal("St. Sebastian", hospital.Name);
            Assert.Equal(2, hospital.IpAddresses.Count);
        }


        [Fact]
        public async Task UpdateHospitalAsync_ShouldSynchronizeIpAddresses()
        {
            // Arrange.
            var hospital = BuildTestHospital(id: 1);
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            // Current IP is 192.168.1.1. We want to remove it and add 1.1.1.1.
            var updateDto = new UpdateHospitalDTO
            {
                IpAddresses = ["1.1.1.1"]
            };

            // Act.
            await _repository.UpdateHospitalAsync(1, updateDto, null);

            // Assert.
            var updated = await _context.Hospitals.Include(h => h.IpAddresses).FirstAsync(h => h.ID == 1);
            Assert.Single(updated.IpAddresses);
            Assert.Equal("1.1.1.1", updated.IpAddresses.First().IpAddress);
            Assert.DoesNotContain(updated.IpAddresses, x => x.IpAddress == "192.168.1.1");
        }


        [Fact]
        public async Task IsIpAuthorisedAsync_ShouldReturnTrue_WhenIpMatches()
        {
            // Arrange.
            var hospital = BuildTestHospital(id: 5);
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            // Act.
            var isAuthorised = await _repository.IsIpAuthorisedAsync(5, "192.168.1.1");
            var isDenied = await _repository.IsIpAuthorisedAsync(5, "8.8.8.8");

            // Assert.
            Assert.True(isAuthorised);
            Assert.False(isDenied);
        }


        [Fact]
        public async Task DeleteHospitalAsync_ShouldRemoveHospitalAndIps()
        {
            // Arrange.
            var hospital = BuildTestHospital(id: 99);
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.DeleteHospitalAsync(99);

            // Assert.
            Assert.True(result);
            Assert.Null(await _context.Hospitals.FindAsync(99));
            // Verify cascade behavior (or manual removal)
            var ips = await _context.HospitalIpAddresses.Where(x => x.HospitalID == 99).ToListAsync();
            Assert.Empty(ips);
        }


        [Fact]
        public async Task UpdateHospitalAsync_ShouldThrow_WhenNotFound()
        {
            // Act & Assert.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.UpdateHospitalAsync(404, new UpdateHospitalDTO { Name = "Ghost" }, null));
        }
    }
}