namespace IntegrityVault.Tests.Controllers;

// Define the test suite for the AuthController.
public class AuthControllerTests
{
    // Define mock dependencies and the controller instance.
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _controller;


    public AuthControllerTests()
    {
        // Initialise the mock service.
        _mockAuthService = new Mock<IAuthService>();

        // Initialise the mock configuration for JWT settings.
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup the mandatory JWT configuration values required by the CreateToken method.
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("SuperSecretTestKey1234567890123456");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("IntegrityVaultTest");

        // Initialise the controller with mocked dependencies.
        _controller = new AuthController(_mockAuthService.Object, _mockConfiguration.Object);
    }


    [Fact]
    public async Task Login_ShouldReturnOk_WithJwtToken_WhenCredentialsAreValid()
    {
        // Arrange: Prepare valid login DTO and a dummy Doctor user (inherits from User).
        var loginDto = new AuthLoginDTO
        {
            UsernameOrEmail = "dr_smith",
            Password = "Password123",
            IpAddress = "127.0.0.1"
        };

        var fakeUser = new Doctor
        {
            ID = 1,
            Username = "dr_smith",
            Email = "smith@hospital.com",
            Password = "hashed_password",
            FirstName = "Jhon",
            LastName = "Smith",
            Role = UserRole.Doctor,
            Specialty = DoctorSpecialty.Cardiology,
            HospitalID = 10
        };

        _mockAuthService.Setup(s => s.LoginAsync(loginDto.UsernameOrEmail, loginDto.Password, loginDto.IpAddress))
            .ReturnsAsync(fakeUser);

        // Act: Execute the login method.
        var result = await _controller.Login(loginDto);

        // Assert: Verify the response is 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Use reflection or dynamic to check the anonymous object { token }.
        var responseValue = okResult.Value;
        var tokenProperty = responseValue?.GetType().GetProperty("token");
        var token = tokenProperty?.GetValue(responseValue) as string;

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }


    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserIsNull()
    {
        // Arrange: Setup service to return null (invalid credentials).
        var loginDto = new AuthLoginDTO { UsernameOrEmail = "wrong", Password = "wrong", IpAddress = "0.0.0.0" };
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act: Execute login.
        var result = await _controller.Login(loginDto);

        // Assert: Verify 401 Unauthorized is returned.
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }


    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenInvalidOperationOccurs()
    {
        // Arrange: Simulate a business logic error (e.g., account locked).
        var loginDto = new AuthLoginDTO { UsernameOrEmail = "locked_user", Password = "Password123", IpAddress = "1.1.1.1" };
        var errorMessage = "Account is temporarily locked due to multiple failed attempts";

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act: Execute login.
        var result = await _controller.Login(loginDto);

        // Assert: Verify 400 BadRequest with the correct message.
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }


    [Fact]
    public async Task Login_ShouldReturnInternalServerError_OnUnexpectedException()
    {
        // Arrange: Simulate a database crash.
        var loginDto = new AuthLoginDTO { UsernameOrEmail = "user", Password = "pw", IpAddress = "1.1.1.1" };
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection timed out"));

        // Act: Execute login.
        var result = await _controller.Login(loginDto);

        // Assert: Verify 500 Internal Server Error.
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error: Database connection timed out.", statusCodeResult.Value);
    }
}