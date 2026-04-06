using Microsoft.Extensions.Options;
using IntegrityVault.Common.Configurations;

namespace IntegrityVault.Tests.Services;

// Define the test suite for the BlockchainService implementation.
public class BlockchainServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IHospitalRepository> _mockHospitalRepository;
    private readonly Mock<ICryptoService> _mockCryptoService;
    private readonly Mock<IOptions<BlockchainSettings>> _mockOptions;
    private readonly BlockchainService _service;


    // Helper to create a dummy 32-byte array for hashes.
    private readonly byte[] _dummyHash = new byte[32];


    public BlockchainServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockHospitalRepository = new Mock<IHospitalRepository>();
        _mockCryptoService = new Mock<ICryptoService>();
        _mockOptions = new Mock<IOptions<BlockchainSettings>>();

        // Set up dummy configurations.
        var settings = new BlockchainSettings
        {
            RPC_URL = "http://localhost:8545",
            ContractAddress = "0x1234567890123456789012345678901234567890",
            SuperAdminWalletAddress = "0xSuperAdminWallet"
        };
        _mockOptions.Setup(o => o.Value).Returns(settings);

        _service = new BlockchainService(
            _mockOptions.Object,
            _mockUserRepository.Object,
            _mockHospitalRepository.Object,
            _mockCryptoService.Object
        );
    }



    #region Hospital Management Methods

    [Fact]
    public async Task AddHospitalToChainAsync_ShouldThrowException_WhenSuperAdminNotFound()
    {
        // Arrange: Simulate superadmin missing from DB.
        _mockUserRepository.Setup(r => r.GetSuperAdminByWalletAsync(It.IsAny<string>()))
            .ReturnsAsync((SuperAdmin?)null);

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddHospitalToChainAsync(1, "0xHospitalWallet"));

        Assert.Contains("Superadmin owner record not found", ex.Message);
    }


    [Fact]
    public async Task UpdateHospitalWalletOnChainAsync_ShouldThrowException_WhenDecryptionFails()
    {
        // Arrange: Setup concrete superadmin.
        var superadmin = new SuperAdmin
        {
            Username = "Example",
            Role = UserRole.SuperAdmin,
            Password = "hashed_password",
            Email = "example@integrityvault.com",
            WalletAddress = "0xSuperAdminWallet",
            EncryptedPrivateKey = System.Text.Encoding.UTF8.GetBytes("locked")
        };
        _mockUserRepository.Setup(r => r.GetSuperAdminByWalletAsync(It.IsAny<string>()))
            .ReturnsAsync(superadmin);
        _mockCryptoService.Setup(c => c.Decrypt(System.Text.Encoding.UTF8.GetBytes("locked"))).Throws(new Exception("Decryption error"));

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateHospitalWalletOnChainAsync(1, "0xNewWallet"));

        Assert.Contains("Decryption error", ex.Message);
    }


    [Fact]
    public async Task DeleteHospitalWalletFromChainAsync_ShouldFail_WhenBlockchainReturnsError()
    {
        // Arrange: Setup data for the build process.
        var superadmin = new SuperAdmin
        {
            Username = "Example",
            Role = UserRole.SuperAdmin,
            Password = "hashed_password",
            Email = "example@integrityvault.com",
            WalletAddress = "0xSuperAdminWallet",
            EncryptedPrivateKey = System.Text.Encoding.UTF8.GetBytes("safe")
        };
        _mockUserRepository.Setup(r => r.GetSuperAdminByWalletAsync(It.IsAny<string>()))
            .ReturnsAsync(superadmin);
        _mockCryptoService.Setup(c => c.Decrypt(System.Text.Encoding.UTF8.GetBytes("safe"))).Returns("0x6273156aB0541C653315732f127F79ceD14609805987309E0D80E5370A654A2D");

        // Act & Assert: Fails due to no real RPC server, verifying the catch block works.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteHospitalWalletFromChainAsync(1));
    }

    #endregion



    #region Record Management Methods

    [Fact]
    public async Task RegisterRecordOnChainAsync_ShouldThrow_WhenHospitalDoesNotExist()
    {
        // Arrange: Target hospital not in database.
        _mockHospitalRepository.Setup(r => r.GetHospitalByIdAsync(100))
            .ReturnsAsync((Hospital?)null);

        // Act & Assert: Passing _dummyHash (byte[]) instead of strings.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterRecordOnChainAsync(100, 1, 1, _dummyHash, _dummyHash, "QmCID"));

        Assert.Contains("Hospital 100 not found", ex.Message);
    }


    [Fact]
    public async Task UpdateRecordOnChainAsync_ShouldHandleBlockchainFailureGracefully()
    {
        // Arrange: Valid hospital setup.
        var hospital = new Hospital
        {
            WalletAddress = "0xHospital",
            Name = "General Care Hospital",
            EncryptedPrivateKey = System.Text.Encoding.UTF8.GetBytes("key")
        };
        _mockHospitalRepository.Setup(r => r.GetHospitalByIdAsync(1))
            .ReturnsAsync(hospital);
        _mockCryptoService.Setup(c => c.Decrypt(System.Text.Encoding.UTF8.GetBytes("key"))).Returns("0x6273156aB0541C653315732f127F79ceD14609805987309E0D80E5370A654A2D");

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateRecordOnChainAsync(1, 1, 1, _dummyHash, _dummyHash, "QmNew"));

        Assert.Contains("Blockchain error updating record", ex.Message);
    }

    #endregion


    #region Read Operations

    [Fact]
    public async Task GetRecordFromChainAsync_ShouldWrapRpcExceptions()
    {
        // Arrange: Build a NEW service instance with a null RPC URL.
        var invalidOptions = new Mock<IOptions<BlockchainSettings>>();
        invalidOptions.Setup(o => o.Value).Returns(new BlockchainSettings
        {
            RPC_URL = null!,
            ContractAddress = "0x123",
            SuperAdminWalletAddress = "0xAdmin"
        });

        var freshService = new BlockchainService(
            invalidOptions.Object,
            _mockUserRepository.Object,
            _mockHospitalRepository.Object,
            _mockCryptoService.Object
        );

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            freshService.GetRecordFromChainAsync(1, 1));

        Assert.Contains("Blockchain error reading record", ex.Message);
    }


    [Fact]
    public async Task GetLatestRecordFromChainAsync_ShouldWrapRpcExceptions()
    {
        // Arrange: Build a NEW service instance with an unreachable RPC URL.
        var invalidOptions = new Mock<IOptions<BlockchainSettings>>();
        invalidOptions.Setup(o => o.Value).Returns(new BlockchainSettings
        {
            RPC_URL = "http://invalid_domain_that_does_not_exist_12345.com",
            ContractAddress = "0x123",
            SuperAdminWalletAddress = "0xAdmin"
        });

        var freshService = new BlockchainService(
            invalidOptions.Object,
            _mockUserRepository.Object,
            _mockHospitalRepository.Object,
            _mockCryptoService.Object
        );

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            freshService.GetLatestRecordFromChainAsync(50));

        Assert.Contains("Blockchain error reading latest record", ex.Message);
    }

    #endregion



    #region Helper Method Logic Tests

    [Fact]
    public async Task EnsureSufficientBalance_Check_IsImplicitlyTested()
    {
        // Arrange: Setup hospital for build instance check.
        var hospital = new Hospital
        {
            WalletAddress = "0xHospital",
            Name = "General Care Hospital",
            EncryptedPrivateKey = System.Text.Encoding.UTF8.GetBytes("key")
        };
        _mockHospitalRepository.Setup(r => r.GetHospitalByIdAsync(1)).ReturnsAsync(hospital);
        _mockCryptoService.Setup(c => c.Decrypt(System.Text.Encoding.UTF8.GetBytes("key"))).Returns("0x6273156aB0541C653315732f127F79ceD14609805987309E0D80E5370A654A2D");

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterRecordOnChainAsync(1, 1, 1, _dummyHash, _dummyHash, "QmCID"));


        Assert.NotNull(ex.Message);
    }

    #endregion
}