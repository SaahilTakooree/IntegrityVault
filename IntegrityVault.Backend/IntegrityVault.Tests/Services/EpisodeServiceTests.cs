using IntegrityVault.Repository.Contexts;


namespace IntegrityVault.Tests.Services;

// Define the test suite for the EpisodeService implementation.
public class EpisodeServiceTests
{
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepo;
    private readonly Mock<IntegrityVaultDbContext> _mockContext;
    private readonly EpisodeService _service;

    public EpisodeServiceTests()
    {
        _mockEpisodeRepo = new Mock<IEpisodeRepository>();

        // Mocking a DbContext requires a valid DbContextOptions.
        var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _mockContext = new Mock<IntegrityVaultDbContext>(options);
        _service = new EpisodeService(_mockEpisodeRepo.Object, _mockContext.Object);
    }



    #region Get Methods

    [Fact]
    public async Task GetEpisodeByIdAsync_ShouldReturnEpisode_WhenIdIsValid()
    {
        // Arrange: Prepare valid DTO and matching entity.
        var dto = new EpisodeIdDTO { ID = 1 };
        var episode = new Episode {
            ID = 1,
            Title = "Cross eye",
            Specialty = DoctorSpecialty.GeneralMedicine,
            CreatedAt = DateTime.Now,
            DoctorID = 10,
            PatientID = 20
        };
        _mockEpisodeRepo.Setup(r => r.GetEpisodeByIdAsync(dto)).ReturnsAsync(episode);

        // Act.
        var result = await _service.GetEpisodeByIdAsync(dto);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(1, result.ID);
    }


    [Fact]
    public async Task GetEpisodeByIdAsync_ShouldThrow_WhenIdIsInvalid()
    {
        // Arrange: ID 0 or negative should trigger the ThrowIfInvalidId helper.
        var dto = new EpisodeIdDTO { ID = 0 };

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetEpisodeByIdAsync(dto));
    }

    #endregion



    #region Create Methods

    [Fact]
    public async Task CreateEpisodeAsync_ShouldReturnDto_AndSaveContext()
    {
        // Arrange.
        var episode = new Episode
        {
            ID = 1,
            Title = "Cross eye",
            Specialty = DoctorSpecialty.GeneralMedicine,
            CreatedAt = DateTime.Now,
            DoctorID = 10,
            PatientID = 20
        };
        var expectedDto = new EpisodeIdDTO { ID = 99 };

        _mockEpisodeRepo.Setup(r => r.CreateEpisodeAsync(episode)).ReturnsAsync(expectedDto);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act.
        var result = await _service.CreateEpisodeAsync(episode);

        // Assert.
        Assert.Equal(99, result.ID);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion



    #region Status Update Methods

    [Fact]
    public async Task SetEpisodeStatusAsync_ShouldReturnTrue_WhenDoctorMatches()
    {
        // Arrange: Doctor 10 owns Episode 1.
        int episodeId = 1;
        int doctorId = 10;
        var episode = new Episode
        {
            ID = 1,
            Title = "Cross eye",
            Specialty = DoctorSpecialty.GeneralMedicine,
            CreatedAt = DateTime.Now,
            DoctorID = 10,
            PatientID = 20
        };

        _mockEpisodeRepo.Setup(r => r.GetEpisodeByIdAsync(It.Is<EpisodeIdDTO>(d => d.ID == episodeId)))
            .ReturnsAsync(episode);
        _mockEpisodeRepo.Setup(r => r.SetEpisodeStatusAsync(episodeId))
            .ReturnsAsync(true);

        // Act.
        var result = await _service.SetEpisodeStatusAsync(episodeId, doctorId);

        // Assert.
        Assert.True(result);
    }


    [Fact]
    public async Task SetEpisodeStatusAsync_ShouldThrow_WhenDoctorDoesNotMatch()
    {
        // Arrange: Episode belongs to Doctor 10, but Doctor 99 tries to update.
        int episodeId = 1;
        var episode = new Episode
        {
            ID = 1,
            Title = "Cross eye",
            Specialty = DoctorSpecialty.GeneralMedicine,
            CreatedAt = DateTime.Now,
            DoctorID = 10,
            PatientID = 20
        };

        _mockEpisodeRepo.Setup(r => r.GetEpisodeByIdAsync(It.IsAny<EpisodeIdDTO>()))
            .ReturnsAsync(episode);

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SetEpisodeStatusAsync(episodeId, 99));

        Assert.Contains("cannot update episode", ex.Message);
    }


    [Fact]
    public async Task SetEpisodeStatusAsync_ShouldThrow_WhenEpisodeNotFound()
    {
        // Arrange.
        _mockEpisodeRepo.Setup(r => r.GetEpisodeByIdAsync(It.IsAny<EpisodeIdDTO>()))
            .ReturnsAsync((Episode?)null);

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SetEpisodeStatusAsync(1, 1));

        Assert.Contains("not found", ex.Message);
    }

    #endregion



    #region General Exception Handling

    [Fact]
    public async Task CreateEpisodeAsync_ShouldWrapExceptions_WhenRepoFails()
    {
        // Arrange.
        _mockEpisodeRepo.Setup(r => r.CreateEpisodeAsync(It.IsAny<Episode>()))
            .ThrowsAsync(new Exception("DB Down"));

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateEpisodeAsync(new Episode{
                ID = 1,
                Title = "Cross eye",
                Specialty = DoctorSpecialty.GeneralMedicine,
                CreatedAt = DateTime.Now,
                DoctorID = 10,
                PatientID = 20
            }));

        Assert.Contains("Error while creating an episode", ex.Message);
    }

    #endregion
}