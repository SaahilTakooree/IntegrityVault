namespace IntegrityVault.Tests.Controllers;


// Define the test suite for the UserController.
public class UserControllerTests
{

    // Define mock service and the controller instance.
    private readonly Mock<IUserService> _mockUserService;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        // Initialise the mock service.
        _mockUserService = new Mock<IUserService>();

        // Initialise the controller with the mocked service.
        _controller = new UserController(_mockUserService.Object);
    }



    #region Get Methods

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithSerialisedPolymorphicUsers()
    {
        // Arrange
        var users = new List<UserDTO>
        {
            new DoctorDTO { ID = 1, Username = "dr_jones", FirstName = "Indiana", Specialty = DoctorSpecialty.Cardiology },
            new PatientDTO { ID = 2, Username = "patient_zero", FirstName = "Alice", LastName = "Smith" }
        }.AsEnumerable();

        _mockUserService.Setup(s => s.GetAllUsersAsync(null)).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedElements = Assert.IsAssignableFrom<IEnumerable<System.Text.Json.JsonElement>>(okResult.Value);

        Assert.Equal(2, returnedElements.Count());

        var firstUserJson = returnedElements.First().GetRawText();
        Assert.Contains("dr_jones", firstUserJson);

        Assert.Contains("0", firstUserJson);
    }


    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange: Setup the service to return null for a non-existent ID.
        int userId = 404;
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO?)null);

        // Act: Attempt to retrieve the user.
        var result = await _controller.GetUserById(userId);

        // Assert: Verify that a 404 NotFound response is returned with the correct message.
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"User with ID {userId} was not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetAllPatientFromHospital_ShouldReturnOk_WithPatientList()
    {

        var patients = new List<Patient>
        {
            new() {
                ID = 10,
                Username = "p1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "SecurePassword123",
                Role = UserRole.Patient,
                DOB = new DateOnly(1990, 1, 1),
                Gender = PatientGender.Male
            }
        };

        //Setup: Return the Entity list.
        _mockUserService
            .Setup(s => s.GetAllPatientFromHospital(1))
            .ReturnsAsync(patients);

        var result = await _controller.GetAllPatientFromHospital(1);

        var okResult = Assert.IsType<OkObjectResult>(result);


        var returnedPatients = Assert.IsAssignableFrom<IEnumerable<Patient>>(okResult.Value);
        Assert.Single(returnedPatients);
    }

    #endregion



    #region Post Methods

    [Fact]
    public async Task CreateDoctor_ShouldReturnOk_WhenDataIsValid()
    {
        // Arrange: Prepare valid doctor creation data.
        var dto = new CreateDoctorDTO
        {
            Username = "doc1",
            Email = "d@h.com",
            Password = "password",
            FirstName = "Marcus",
            LastName = "Welby",
            Specialty = DoctorSpecialty.Cardiology
        };
        _mockUserService.Setup(s => s.CreateDoctorAsync(dto)).ReturnsAsync(true);

        // Act: Post to the doctor endpoint.
        var result = await _controller.CreateDoctor(dto);

        // Assert: Verify success.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }


    [Fact]
    public async Task CreateSuperAdmin_ShouldReturnOk_WhenBlockchainDataIsValid()
    {
        // Arrange: Prepare super admin data.
        var dto = new CreateSuperAdminDTO
        {
            Username = "root",
            Email = "r@v.com",
            Password = "secure",
            WalletAddress = "0x742d...",
            PrivateKey = "secret_key"
        };
        _mockUserService.Setup(s => s.CreateSuperAdminAsync(dto)).ReturnsAsync(true);

        // Act: Post to the super admin endpoint.
        var result = await _controller.CreateSuperAdmin(dto);

        // Assert: Verify success.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    #endregion



    #region Patch and Exception Edge Cases

    [Fact]
    public async Task UpdatePatient_ShouldReturnBadRequest_WhenEmailIsDuplicate()
    {
        // Arrange: Simulate a business rule violation (Edge Case).
        var updateDto = new UpdatePatientDTO { Email = "exists@h.com" };
        var error = "Email address is already in use.";
        _mockUserService.Setup(s => s.UpdatePatientAsync(1, updateDto))
            .ThrowsAsync(new InvalidOperationException(error));

        // Act: Execute update.
        var result = await _controller.UpdatePatient(1, updateDto);

        // Assert: Verify 400 BadRequest.
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(error, badRequest.Value);
    }


    [Fact]
    public async Task DeleteUser_ShouldReturnInternalServerError_WhenServiceFails()
    {
        // Arrange: Simulate a critical failure. 
        var exceptionMessage = "Connectivity issue";
        _mockUserService.Setup(s => s.DeleteUserAsync(50))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act: Attempt to delete.
        var result = await _controller.DeleteUser(50);

        // Assert: Verify 500 status. 
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal($"Internal server error: {exceptionMessage}.", objectResult.Value);
    }

    #endregion
}