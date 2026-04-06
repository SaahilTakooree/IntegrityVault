namespace IntegrityVault.Tests.Controllers;


// Define the test suite for the HospitalController.
public class HospitalControllerTests
{
    // Define mock service and the controller instance.
    private readonly Mock<IHospitalService> _mockHospitalService;
    private readonly HospitalController _controller;

    public HospitalControllerTests()
    {
        // Initialise the mock service.
        _mockHospitalService = new Mock<IHospitalService>();

        // Initialise the controller with the mocked service.
        _controller = new HospitalController(_mockHospitalService.Object);
    }



    #region Get Methods

    [Fact]
    public async Task GetAllHospital_ShouldReturnOk_WithListOfHospitals()
    {
        // Arrange: Define a fake list of hospitals to be returned by the service.
        var hospitals = new List<HospitalDTO>
        {
            new() { ID = 1, Name = "St. Mary's", WalletAddress = "0x123", IpAddresses = ["127.0.0.1"] },
            new() { ID = 2, Name = "General Hospital", WalletAddress = "0x456", IpAddresses = ["192.168.1.1"] }
        };
        _mockHospitalService.Setup(s => s.GetAllHospitalsAsync()).ReturnsAsync(hospitals);

        // Act: Call the controller method.
        var result = await _controller.GetAllHospital();

        // Assert: Verify the response is 200 OK using standard xUnit Asserts.
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedHospitals = Assert.IsAssignableFrom<IEnumerable<HospitalDTO>>(okResult.Value);
        Assert.Equal(2, returnedHospitals.Count());
    }


    [Fact]
    public async Task GetHospitalById_ShouldReturnNotFound_WhenHospitalDoesNotExist()
    {
        // Arrange: Setup the service to return null for a non-existent ID.
        int nonExistentId = 999;
        _mockHospitalService.Setup(s => s.GetHospitalByIdAsync(nonExistentId)).ReturnsAsync((HospitalDTO?)null);

        // Act: Attempt to retrieve the hospital.
        var result = await _controller.GetHospitalById(nonExistentId);

        // Assert: Verify that a 404 NotFound response is returned.
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Hospital with ID {nonExistentId} was not found.", notFoundResult.Value);
    }

    #endregion


    #region Post Methods

    [Fact]
    public async Task CreateHospital_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange: Define the DTO for creating a hospital.
        var createDto = new CreateHospitalDTO
        {
            Name = "New Clinic",
            WalletAddress = "0xABC",
            PrivateKey = "priv_123",
            IpAddresses = ["10.0.0.1"]
        };
        _mockHospitalService.Setup(s => s.CreateHospitalAsync(createDto)).ReturnsAsync(true);

        // Act: Post the DTO to the controller.
        var result = await _controller.CreateHospital(createDto);

        // Assert: Check for a 200 OK status.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, okResult.Value);
    }

    #endregion



    #region Patch Methods

    [Fact]
    public async Task UpdateHospital_ShouldReturnBadRequest_WhenOperationIsInvalid()
    {
        // Arrange: Simulate an InvalidOperationException from the service.
        var updateDto = new UpdateHospitalDTO { Name = "Updated Name" };
        var errorMessage = "Cannot update a deactivated hospital";

        _mockHospitalService.Setup(s => s.UpdateHospitalAsync(1, updateDto))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act: Execute the update.
        var result = await _controller.UpdateHospital(1, updateDto);

        // Assert: Verify 400 BadRequest matches your controller's catch block.
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }

    #endregion



    #region Delete Methods

    [Fact]
    public async Task DeleteHospital_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange: Simulate an unexpected system failure.
        var exceptionMessage = "Database connection failed";
        _mockHospitalService.Setup(s => s.DeleteHospitalAsync(1))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act: Attempt to delete.
        var result = await _controller.DeleteHospital(1);

        // Assert: Verify the 500 status code and the exact string format from your controller.
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        // Matches: $"Internal server error: {ex.Message}."
        Assert.Equal($"Internal server error: {exceptionMessage}.", statusCodeResult.Value);
    }

    #endregion
}