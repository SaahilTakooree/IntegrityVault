using IntegrityVault.Repository.Contexts;

namespace IntegrityVault.Tests.Repository
{
    public class EpisodeRepositoryTests
    {
        private readonly IntegrityVaultDbContext _context;
        private readonly EpisodeRepository _repository;


        public EpisodeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new IntegrityVaultDbContext(options);
            _repository = new EpisodeRepository(_context);
        }

        #region Helpers


        private static Episode BuildTestEpisode(int id = 1, bool isActive = true) => new()
        {
            ID = id,
            PatientID = 10,
            DoctorID = 11,
            Specialty = DoctorSpecialty.Pediatrics,
            Title = "Test Title",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        #endregion



        [Fact]
        public async Task GetEpisodeByIdAsync_ShouldReturnEpisode_WhenIdExists()
        {
            // Arrange.
            var episode = BuildTestEpisode(id: 50);
            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.GetEpisodeByIdAsync(new EpisodeIdDTO { ID = 50 });

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(50, result.ID);
        }


        [Fact]
        public async Task GetEpisodeByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Act.
            var result = await _repository.GetEpisodeByIdAsync(new EpisodeIdDTO { ID = 999 });

            // Assert.
            Assert.Null(result);
        }


        [Fact]
        public async Task CreateEpisodeAsync_ShouldAddEpisodeAndReturnId()
        {
            // Arrange.
            var episode = BuildTestEpisode(id: 100);

            // Act.
            var result = await _repository.CreateEpisodeAsync(episode);
            await _context.SaveChangesAsync(); // Repository adds, we save to verify.

            // Assert.
            var dbEpisode = await _context.Episodes.FindAsync(100);
            Assert.NotNull(dbEpisode);
            Assert.Equal(100, result.ID);
        }


        [Fact]
        public async Task SetEpisodeStatusAsync_ShouldToggleActiveState()
        {
            // Arrange.
            var episode = BuildTestEpisode(id: 1, isActive: true);
            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            // Act.
            var result = await _repository.SetEpisodeStatusAsync(1);

            // Assert.
            Assert.True(result);
            var updatedEpisode = await _context.Episodes.FindAsync(1);
            Assert.False(updatedEpisode!.IsActive); // Should have toggled from True to False.
        }


        [Fact]
        public async Task SetEpisodeStatusAsync_ShouldThrow_WhenEpisodeNotFound()
        {
            // Act & Assert.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.SetEpisodeStatusAsync(888));

            Assert.Contains("Episode with ID 888 not found", ex.Message);
        }


        [Fact]
        public async Task IsEpisodeActiveAsync_ShouldReturnCurrentStatus()
        {
            // Arrange.
            var activeEpisode = BuildTestEpisode(id: 10, isActive: true);
            var inactiveEpisode = BuildTestEpisode(id: 11, isActive: false);
            _context.Episodes.AddRange(activeEpisode, inactiveEpisode);
            await _context.SaveChangesAsync();

            // Act.
            var isActiveResult = await _repository.IsEpisodeActiveAsync(10);
            var isInactiveResult = await _repository.IsEpisodeActiveAsync(11);

            // Assert.
            Assert.True(isActiveResult);
            Assert.False(isInactiveResult);
        }


        [Fact]
        public async Task IsEpisodeActiveAsync_ShouldThrow_WhenEpisodeDoesNotExist()
        {
            // Act & Assert.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.IsEpisodeActiveAsync(777));

            Assert.Contains("Episode with ID 777 not found", ex.Message);
        }
    }
}