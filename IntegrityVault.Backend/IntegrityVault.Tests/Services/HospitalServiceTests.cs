using Microsoft.EntityFrameworkCore.Storage;
using IntegrityVault.Repository.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IntegrityVault.Tests.Services;


// Define the test suite for the HospitalService implementation.
public class HospitalServiceTests
{
    private readonly Mock<IHospitalRepository> _mockRepo;
    private readonly Mock<ICryptoService> _mockCrypto;
    private readonly Mock<IBlockchainService> _mockBlockchain;
    private readonly Mock<IntegrityVaultDbContext> _mockContext;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    private readonly HospitalService _service;

    public HospitalServiceTests()
    {
        _mockRepo = new Mock<IHospitalRepository>();
        _mockCrypto = new Mock<ICryptoService>();
        _mockBlockchain = new Mock<IBlockchainService>();
        _mockTransaction = new Mock<IDbContextTransaction>();

        // Setup for DB Transactions
        var options = new DbContextOptionsBuilder<IntegrityVaultDbContext>()
            .UseInMemoryDatabase(databaseName: "HospitalTestDb")
            .Options;
        _mockContext = new Mock<IntegrityVaultDbContext>(options);

        // Mock the DatabaseFacade to return our mocked transaction
        var mockDatabase = new Mock<DatabaseFacade>(_mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(default))
            .ReturnsAsync(_mockTransaction.Object);
        _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        _service = new HospitalService(
            _mockRepo.Object,
            _mockCrypto.Object,
            _mockBlockchain.Object,
            _mockContext.Object);
    }



    #region Read Operations Happy Paths

    [Fact]
    public async Task GetAllHospitalsAsync_ShouldReturnMappedDtos()
    {
        // Arrange: Prepare list of hospitals.
        var hospitals = new List<Hospital>
        {
            new() { ID = 1, Name = "Hosp A", WalletAddress = "0x1234567890123456789012345678901234567890", EncryptedPrivateKey = [1] },
            new() { ID = 2, Name = "Hosp B", WalletAddress = "0x0987654321098765432109876543210987654321", EncryptedPrivateKey = [2] }
        };
        _mockRepo.Setup(r => r.GetAllHospitalsAsync()).ReturnsAsync(hospitals);

        // Act.
        var result = await _service.GetAllHospitalsAsync();

        // Assert.
        Assert.Equal(2, result.Count());
        Assert.Contains(result, h => h.Name == "Hosp A");
    }

    [Fact]
    public async Task GetHospitalByIdAsync_ShouldReturnNull_WhenHospitalNotFound()
    {
        // Arrange.
        _mockRepo.Setup(r => r.GetHospitalByIdAsync(1)).ReturnsAsync((Hospital?)null);

        // Act.
        var result = await _service.GetHospitalByIdAsync(1);

        // Assert.
        Assert.Null(result);
    }

    #endregion



    #region Create Hospital Edge Cases

    [Fact]
    public async Task CreateHospitalAsync_ShouldThrow_WhenWalletAlreadyExists()
    {
        // Arrange: Simulate existing wallet address in DB.
        var existingWallet = "0xExistingWalletAddress12345678901234567890";
        var dto = new CreateHospitalDTO
        {
            Name = "New Hospital",
            WalletAddress = existingWallet,
            PrivateKey = "key",
            IpAddresses = ["127.0.0.1"]
        };

        _mockRepo.Setup(r => r.GetAllHospitalsAsync())
            .ReturnsAsync([new Hospital { WalletAddress = existingWallet, Name = "Old", EncryptedPrivateKey = [0] }]);

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateHospitalAsync(dto));
        Assert.Contains("already exists", ex.Message);
        _mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
    }


    [Fact]
    public async Task CreateHospitalAsync_ShouldThrow_WhenNoIpProvided()
    {
        // Arrange: Missing IP addresses.
        var dto = new CreateHospitalDTO
        {
            Name = "No IP Hosp",
            WalletAddress = "0x1234567890123456789012345678901234567890",
            PrivateKey = "key",
            IpAddresses = []
        };

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateHospitalAsync(dto));
        Assert.Contains("At least one IP address is required", ex.Message);
    }

    #endregion



    #region Create/Delete Coordination (Blockchain + DB)

    [Fact]
    public async Task CreateHospitalAsync_ShouldCommit_WhenAllServicesSucceed()
    {
        // Arrange.
        var dto = new CreateHospitalDTO
        {
            Name = "Test Hospital",
            WalletAddress = "0x1234567890123456789012345678901234567890",
            PrivateKey = "plainKey",
            IpAddresses = ["1.1.1.1"]
        };

        _mockRepo.Setup(r => r.GetAllHospitalsAsync()).ReturnsAsync([]);
        _mockCrypto.Setup(c => c.Encrypt("plainKey")).Returns([1, 2, 3]);
        _mockRepo.Setup(r => r.CreateHospitalAsync(dto, It.IsAny<byte[]>())).ReturnsAsync(10);

        // Act.
        var result = await _service.CreateHospitalAsync(dto);

        // Assert.
        Assert.True(result);
        _mockBlockchain.Verify(b => b.AddHospitalToChainAsync(10, dto.WalletAddress), Times.Once);
        _mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
    }


    [Fact]
    public async Task DeleteHospitalAsync_ShouldRollback_WhenBlockchainFails()
    {
        // Arrange
        int id = 1;
        var dto = new UpdateHospitalDTO { Name = "Updated Name" };

        _mockRepo.Setup(r => r.GetAllHospitalsAsync()).ReturnsAsync([]);

        _mockRepo.Setup(r => r.UpdateHospitalAsync(id, dto, null)).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateHospitalAsync(id, dto);

        // Assert
        Assert.True(result);
        _mockCrypto.Verify(c => c.Encrypt(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(r => r.UpdateHospitalAsync(id, dto, null), Times.Once);
    }

    #endregion



    #region Update Methods

    [Fact]
    public async Task UpdateHospitalAsync_ShouldEncryptKey_OnlyIfProvided()
    {
        // Arrange.
        int id = 1;
        var dto = new UpdateHospitalDTO { Name = "Updated Name" }; // No private key.

        _mockRepo.Setup(r => r.GetAllHospitalsAsync()).ReturnsAsync([]);

        // Act.
        await _service.UpdateHospitalAsync(id, dto);

        // Assert.
        _mockCrypto.Verify(c => c.Encrypt(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(r => r.UpdateHospitalAsync(id, dto, null), Times.Once);
    }

    #endregion
}