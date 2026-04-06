namespace IntegrityVault.Tests.Services;

// Define the test suite for the AuthService implementation.
public class AuthServiceTests
{
    // Define mock repositories and the service instance.
    private readonly Mock<IAuthRepository> _mockAuthRepository;
    private readonly Mock<IHospitalRepository> _mockHospitalRepository;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // Initialise the mock repositories.
        _mockAuthRepository = new Mock<IAuthRepository>();
        _mockHospitalRepository = new Mock<IHospitalRepository>();

        // Initialise the service with the mocked repositories.
        _service = new AuthService(_mockAuthRepository.Object, _mockHospitalRepository.Object);
    }

    #region Login Happy Paths

    [Theory]
    [InlineData(UserRole.SuperAdmin)]
    [InlineData(UserRole.Patient)]
    public async Task LoginAsync_ShouldReturnUser_WhenRoleIsSuperAdminOrPatient(UserRole role)
    {
        // Arrange: Use a concrete class (Doctor) since User is abstract, and set required members.
        var user = new Doctor
        {
            Role = role,
            Username = "testuser",
            Password = "hashed_password",
            Email = "test@integrityvault.com",
            Specialty = DoctorSpecialty.GeneralMedicine,
            LastName = "Smith",
            FirstName = "Todd"

        };

        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("testuser", "password123"))
            .ReturnsAsync(user);

        // Act: Attempt login with any IP.
        var result = await _service.LoginAsync("testuser", "password123", "192.168.1.1");

        // Assert: Verify the user is returned successfully.
        Assert.NotNull(result);
        Assert.Equal(role, result.Role);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUser_WhenDoctorIpIsAuthorised()
    {
        // Arrange: Prepare a concrete doctor tied to a hospital with a valid IP.
        var user = new Doctor
        {
            Role = UserRole.Doctor,
            HospitalID = 10,
            Username = "dr_smith",
            Password = "hashed_password",
            Email = "smith@hospital.com",
            Specialty = DoctorSpecialty.Pediatrics,
            LastName = "Smith",
            FirstName = "Todd"
        };

        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("dr_smith", "pass"))
            .ReturnsAsync(user);
        _mockHospitalRepository.Setup(r => r.IsIpAuthorisedAsync(10, "10.0.0.1"))
            .ReturnsAsync(true);

        // Act: Attempt login.
        var result = await _service.LoginAsync("dr_smith", "pass", "10.0.0.1");

        // Assert: Verify doctor is logged in.
        Assert.NotNull(result);
        Assert.Equal(UserRole.Doctor, result.Role);
    }

    #endregion

    #region Login Edge Cases (IP Restrictions)

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenAdminIpIsNotAuthorised()
    {
        // Arrange: Admin login (using Doctor as concrete proxy if Admin is also abstract) with an unauthorised IP.
        var user = new Doctor
        {
            Role = UserRole.Admin,
            HospitalID = 5,
            Username = "admin",
            Password = "password",
            Email = "admin@vault.com",
            Specialty = DoctorSpecialty.Pediatrics,
            LastName = "Smith",
            FirstName = "Todd"
        };

        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("admin", "adminpass"))
            .ReturnsAsync(user);
        _mockHospitalRepository.Setup(r => r.IsIpAuthorisedAsync(5, "1.1.1.1"))
            .ReturnsAsync(false);

        // Act: Attempt login.
        var result = await _service.LoginAsync("admin", "adminpass", "1.1.1.1");

        // Assert: Verify login is denied (returns null).
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange: Repository returns null for invalid credentials.
        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("fake", "fake"))
            .ReturnsAsync((User?)null);

        // Act: Attempt login.
        var result = await _service.LoginAsync("fake", "fake", "127.0.0.1");

        // Assert: Verify null is returned.
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenDoctorHasNoHospitalID()
    {
        // Arrange: A doctor record that is missing a HospitalID.
        var user = new Doctor
        {
            Role = UserRole.Doctor,
            HospitalID = null,
            Username = "dr_no_hosp",
            Password = "password",
            Email = "error@hospital.com",
            Specialty = DoctorSpecialty.Pediatrics,
            LastName = "Smith",
            FirstName = "Todd"
        };

        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("dr_no_hosp", "pass"))
            .ReturnsAsync(user);

        // Act: Attempt login.
        var result = await _service.LoginAsync("dr_no_hosp", "pass", "192.168.1.1");

        // Assert: Verify login is denied because HospitalID is required for IP check.
        Assert.Null(result);
    }

    #endregion

    #region External Provider Logic

    [Fact]
    public async Task LoginAsync_ShouldHandleExternalProvider_Correctiy()
    {
        // Arrange: ExternalProvider concrete class.
        var external = new ExternalProvider
        {
            Role = UserRole.ExternalProvider,
            BelongsToID = 99,
            Username = "ext_user",
            Password = "password",
            Email = "ext@provider.com"
        };

        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync("ext_user", "ext_pass"))
            .ReturnsAsync(external);
        _mockHospitalRepository.Setup(r => r.IsIpAuthorisedAsync(99, "8.8.8.8"))
            .ReturnsAsync(true);

        // Act: Attempt login.
        var result = await _service.LoginAsync("ext_user", "ext_pass", "8.8.8.8");

        // Assert: Verify successful login and correct type.
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ExternalProvider>(result);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidOperationException_OnRepositoryFailure()
    {
        // Arrange: Simulate a database crash.
        var dbError = "Connection failed";
        _mockAuthRepository.Setup(r => r.GetUserByCredentialAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception(dbError));

        // Act & Assert: Verify the custom exception message is thrown.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.LoginAsync("user", "pass", "127.0.0.1"));

        Assert.Contains($"Error while trying to fetch to log a user in: {dbError}.", exception.Message);
    }

    #endregion
}