using Moq;
using IntegrityVault.Service.Implementations;
using IntegrityVault.Repository.Interfaces;
using IntegrityVault.Service.Interfaces;
using IntegrityVault.Common.Entities;
using IntegrityVault.Common.DTOs;
using IntegrityVault.Common.Enums;

namespace IntegrityVault.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IHospitalRepository> _mockHospitalRepo;
    private readonly Mock<ICryptoService> _mockCrypto;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockHospitalRepo = new Mock<IHospitalRepository>();
        _mockCrypto = new Mock<ICryptoService>();
        _service = new UserService(_mockUserRepo.Object, _mockHospitalRepo.Object, _mockCrypto.Object);
    }

    #region Get Methods Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnMappedDTOs()
    {
        // Arrange
        var users = new List<User>
        {
            new Doctor { ID = 1, Username = "doc1", Email = "doc@test.com", Role = UserRole.Doctor, Password = "Qwerty!2", Specialty = DoctorSpecialty.Cardiology, LastName = "Joe" , FirstName = "Sam" },
            new Patient { ID = 2, Username = "pat1", Email = "pat@test.com", Role = UserRole.Patient, Password = "Qwerty!2", FirstName = "John", Gender = PatientGender.Male, LastName = "Joe", DOB = new DateOnly(1990, 1, 1) }
        };
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.IsType<DoctorDTO>(result.First());
        Assert.IsType<PatientDTO>(result.Last());
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllPatientFromHospital_ShouldThrow_WhenHospitalNotFound()
    {
        // Arrange
        _mockHospitalRepo.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllPatientFromHospital(1));
        Assert.Contains("Hospital cannot be found", ex.Message);
    }

    #endregion

    #region Creation Tests (Doctor & SuperAdmin focus)

    [Fact]
    public async Task CreateDoctorAsync_ShouldThrow_WhenEmailExists()
    {
        // Arrange
        var dto = new CreateDoctorDTO { Email = "exists@test.com", Username = "new", Password = "123", FirstName = "A", LastName = "B", Specialty = DoctorSpecialty.Pediatrics };

        // Mock email check (DoesEmailExist calls GetAllUsersAsync internally)
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync(
            [
                new Doctor
                {
                    ID = 2,
                    Email = "exists@test.com",
                    Username = "otherExt",
                    Password = "pw",
                    FirstName = "X",
                    LastName = "Z",
                    Specialty = DoctorSpecialty.Pediatrics,
                    Role = UserRole.Doctor
                }
            ]
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateDoctorAsync(dto));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task CreateSuperAdminAsync_ShouldEncryptPrivateKey()
    {
        // Arrange
        var dto = new CreateSuperAdminDTO
        {
            Username = "admin",
            Email = "s@test.com",
            Password = "pw",
            WalletAddress = "0x123",
            PrivateKey = "secret_key"
        };
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync([]);
        _mockCrypto.Setup(c => c.Encrypt(dto.PrivateKey)).Returns([1, 1, 1]);
        _mockUserRepo.Setup(r => r.CreateSuperAdminAsync(dto, It.IsAny<byte[]>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateSuperAdminAsync(dto);

        // Assert
        Assert.True(result);
        _mockCrypto.Verify(c => c.Encrypt("secret_key"), Times.Once);
    }

    #endregion

    #region External Provider Specific Logic

    [Fact]
    public async Task CreateExternalProviderAsync_ShouldThrow_WhenBelongsToSameAsHospital()
    {
        // Arrange
        var dto = new CreateExternalProviderDTO
        {
            Username = "ext",
            Email = "e@t.com",
            Password = "p",
            HospitalID = 1,
            BelongsToID = 1
        };
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync([]);
        _mockHospitalRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateExternalProviderAsync(dto));
        Assert.Contains("cannot be the same", ex.Message);
    }

    #endregion

    #region Update & Delete Tests

    [Fact]
    public async Task UpdateDoctorAsync_ShouldHashPassword_WhenProvided()
    {
        // Arrange
        var updateDto = new UpdateDoctorDTO { Password = "newPassword" };
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync([]);
        _mockUserRepo.Setup(r => r.UpdateDoctorAsync(1, It.IsAny<UpdateDoctorDTO>())).ReturnsAsync(true);

        // Act
        await _service.UpdateDoctorAsync(1, updateDto);

        // Assert
        // Verify hashing happened (password should no longer be "newPassword")
        Assert.NotEqual("newPassword", updateDto.Password);
        _mockUserRepo.Verify(r => r.UpdateDoctorAsync(1, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldCallRepo_WhenIdIsValid()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.DeleteUserAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteUserAsync(1);

        // Assert
        Assert.True(result);
        _mockUserRepo.Verify(r => r.DeleteUserAsync(1), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrow_WhenIdIsInvalid()
    {
        // Assuming ThrowIfInvalidId throws for 0 or negative
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetUserByIdAsync(0));
    }

    [Fact]
    public async Task UpdateExternalProviderAsync_ShouldThrow_WhenEmailTakenByOther()
    {
        // Arrange
        var dto = new UpdateExternalProviderDTO { Email = "taken@test.com" };
        _mockUserRepo.Setup(r => r.GetAllUsersAsync(null)).ReturnsAsync(
            [
                new ExternalProvider
                {
                    ID = 2,
                    Email = "taken@test.com",
                    Username = "otherExt",
                    Password = "pw",
                    Role = UserRole.ExternalProvider
                }
            ]
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateExternalProviderAsync(1, dto));
        Assert.Contains("already exists", ex.Message);
    }

    #endregion
}