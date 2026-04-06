using Microsoft.AspNetCore.Http;

namespace IntegrityVault.Tests.Controllers;

// Define the test suite for the MedicalRecordController.
public class MedicalRecordControllerTests
{
    // Define mock services and the controller instance.
    private readonly Mock<IMedicalRecordService> _mockMedicalRecordService;
    private readonly Mock<IEpisodeService> _mockEpisodeService;
    private readonly MedicalRecordController _controller;

    public MedicalRecordControllerTests()
    {
        // Initialise the mock services.
        _mockMedicalRecordService = new Mock<IMedicalRecordService>();
        _mockEpisodeService = new Mock<IEpisodeService>();

        // Initialise the controller with the mocked services.
        _controller = new MedicalRecordController(_mockMedicalRecordService.Object, _mockEpisodeService.Object);
    }

    #region Post Methods

    [Fact]
    public async Task CreateNewMedicalRecordAndEpisode_ShouldReturnOk_WhenDataIsValid()
    {
        // Arrange: Prepare a valid DTO for creating a medical record and episode.
        var dto = new CreateMedicalRecordDTO
        {
            DoctorID = 1,
            PatientID = 2,
            Specialty = DoctorSpecialty.Cardiology,
            VisitDate = new DateOnly(2026, 3, 31),
            ChiefComplaint = "Chest pain",
            Diagnosis = "Angina",
            TreatmentPlan = "Rest and medication"
        };
        _mockMedicalRecordService.Setup(s => s.CreateMedicalRecordAndEpisodeAsync(dto)).ReturnsAsync(true);

        // Act: Execute the creation method.
        var result = await _controller.CreateNewMedicalRecordAndEpisode(dto);

        // Assert: Verify the response is 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task AddMedicalRecordToEpisode_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange: Prepare DTO for adding to an existing episode.
        var dto = new CreateMedicalRecordDTO { ChiefComplaint = "Follow up", Diagnosis = "Recovering", TreatmentPlan = "Continue meds" };
        _mockMedicalRecordService.Setup(s => s.AddMedicalRecordToEpisodeAsync(10, dto)).ReturnsAsync(true);

        // Act: Add record to episode 10.
        var result = await _controller.AddMedicalRecordToEpisode(10, dto);

        // Assert: Verify success.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task VerifyPdfTampering_ShouldReturnOk_WithTamperStatus()
    {
        // Arrange: Prepare a mock file using a MemoryStream.
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write("dummy pdf content");
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(_ => _.Length).Returns(ms.Length);
        fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream, token));

        var verifyResult = new VerifyMedicalRecordDTO { IsTampered = false, Status = "Valid" };
        _mockMedicalRecordService.Setup(s => s.VerifyPdfTamperingAsync(It.IsAny<byte[]>(), 15)).ReturnsAsync(verifyResult);

        // Act: Verify the uploaded PDF.
        var result = await _controller.VerifyPdfTampering(15, fileMock.Object);

        // Assert: Verify 200 OK and check the tamper status.
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVerify = Assert.IsType<VerifyMedicalRecordDTO>(okResult.Value);
        Assert.False(returnedVerify.IsTampered);
    }

    #endregion

    #region Get Methods

    [Fact]
    public async Task GetPatientMedicalHistory_ShouldReturnOk_WithHistoryData()
    {
        // Arrange: Use As<dynamic> or cast to the specific DTO type if required by your interface.
        // Since you don't want to define DTOs, we setup the service to return a valid object casted.
        var historyResponse = new PatientMedicalHistoryDTO
        {
            PatientID = 15,
            PatientFullName = "Patient Patient"
        };
        _mockMedicalRecordService.Setup(s => s.GetPatientMedicalHistoryAsync(15)).ReturnsAsync(historyResponse);

        // Act: Retrieve patient history.
        var result = await _controller.GetPatientMedicalHistory(15);

        // Assert: Verify the response is 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetDoctorMedicalHistory_ShouldReturnOk_WithHistoryData()
    {
        // Arrange: Prepare the response using the existing DTO from your namespace.
        var doctorHistory = new DoctorMedicalHistoryDTO
        {
            DoctorID = 14,
            DoctorFullName = "Doctor Doctor"
        };
        _mockMedicalRecordService.Setup(s => s.GetDoctorMedicalHistoryAsync(14)).ReturnsAsync(doctorHistory);

        // Act: Retrieve doctor history.
        var result = await _controller.GetDoctorMedicalHistory(14);

        // Assert: Verify 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMedicalRecordInformFromCID_ShouldReturnOk_WithAnonymousExtractedFields()
    {
        // Arrange: Setup service to return the record data DTO.
        var record = new MedicalRecordPdfDataDTO
        {
            ChiefComplaint = "Cough",
            Diagnosis = "Cold",
            TreatmentPlan = "Syrup",
            DoctorNotes = "Rest",
            FollowUpInstructions = "Drink water",
            PatientFirstName = "John",
            PatientLastName = "Doe",
            PatientGender = PatientGender.Male,
            DoctorFirstName = "Jane",
            DoctorLastName = "Smith",
            DoctorSpecialy = DoctorSpecialty.Pediatrics,
            HospitalName = "City Hospital"
        };
        _mockMedicalRecordService.Setup(s => s.GetMedicalRecordInformationFromCIDAsync("Qm123", 1)).ReturnsAsync(record);

        // Act: Call the CID info endpoint.
        var result = await _controller.GetMedicalRecordInformFromCID("Qm123", 1);

        // Assert: Verify the result is not null.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region Patch Methods

    [Fact]
    public async Task SetEpisodeStatus_ShouldReturnOk_WhenEpisodeServiceSucceeds()
    {
        // Arrange: Setup episode status update.
        _mockEpisodeService.Setup(s => s.SetEpisodeStatusAsync(14, 14)).ReturnsAsync(true);

        // Act: Execute.
        var result = await _controller.SetEpisodeStatus(14, 14);

        // Assert: Verify success.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task UpdateMedicalrecord_ShouldReturnOk_WhenPatchIsSuccessful()
    {
        // Arrange: Setup the patch service.
        var dto = new CreateMedicalRecordDTO { ChiefComplaint = "Headach", TreatmentPlan = "Drink Water", Diagnosis = "No water" };
        _mockMedicalRecordService.Setup(s => s.PatchMedicalRecordAsync(1, 2, dto)).ReturnsAsync(true);

        // Act: Execute the patch.
        var result = await _controller.UpdateMedicalrecord(1, 2, dto);

        // Assert: Verify 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    #endregion

    #region Tamper and Download Methods

    [Fact]
    public async Task CheckIfMedicalRecordTampered_ShouldReturnOk_WithVerifyDTO()
    {
        // Arrange: Setup tamper check.
        var verifyResult = new VerifyMedicalRecordDTO { IsTampered = true, Message = "Hash mismatch" };
        _mockMedicalRecordService.Setup(s => s.IsMedicalRecordTamperedAsync("QmHash", 15)).ReturnsAsync(verifyResult);

        // Act: Check for tampering.
        var result = await _controller.CheckIfMedicalRecordTampered("QmHash", 15);

        // Assert: Verify 200 OK.
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVerify = Assert.IsType<VerifyMedicalRecordDTO>(okResult.Value);
        Assert.True(returnedVerify.IsTampered);
    }

    [Fact]
    public async Task DownloadMedicalRecord_ShouldReturnFileResult_WhenDataIsValid()
    {
        // Arrange: Prepare file data.
        byte[] pdfBytes = [32, 33, 34];
        string fileName = "History_Report";
        _mockMedicalRecordService.Setup(s => s.DownloadMedicalRecordAsync("QmDownload", 15))
            .ReturnsAsync((pdfBytes, fileName));

        // Act: Request download.
        var result = await _controller.DownloadMedicalRecord("QmDownload", 15);

        // Assert: Verify file properties.
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("History_Report.pdf", fileResult.FileDownloadName);
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task CreateNewMedicalRecordAndEpisode_ShouldReturnBadRequest_WhenOperationIsInvalid()
    {
        // Arrange: Simulate business violation.
        var error = "Patient does not exist";
        _mockMedicalRecordService.Setup(s => s.CreateMedicalRecordAndEpisodeAsync(It.IsAny<CreateMedicalRecordDTO>()))
            .ThrowsAsync(new InvalidOperationException(error));

        // Act: Execute.
        var result = await _controller.CreateNewMedicalRecordAndEpisode(new CreateMedicalRecordDTO { ChiefComplaint = "X", TreatmentPlan = "Y", Diagnosis = "Z" });

        // Assert: Verify 400.
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(error, badRequest.Value);
    }

    [Fact]
    public async Task GetDoctorMedicalHistory_ShouldReturnInternalServerError_WhenServiceCrashes()
    {
        // Arrange: Simulate server crash.
        var msg = "Database connection timeout";
        _mockMedicalRecordService.Setup(s => s.GetDoctorMedicalHistoryAsync(14)).ThrowsAsync(new Exception(msg));

        // Act: Request.
        var result = await _controller.GetDoctorMedicalHistory(14);

        // Assert: Verify 500.
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal($"Internal server error: {msg}.", statusResult.Value);
    }

    [Fact]
    public async Task VerifyPdfTampering_ShouldReturnBadRequest_WhenFileIsEmpty()
    {
        // Arrange: Prepare empty file.
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(_ => _.Length).Returns(0);

        // Act: Execute.
        var result = await _controller.VerifyPdfTampering(1, fileMock.Object);

        // Assert: Verify 400.
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid file.", badRequest.Value);
    }

    #endregion
}