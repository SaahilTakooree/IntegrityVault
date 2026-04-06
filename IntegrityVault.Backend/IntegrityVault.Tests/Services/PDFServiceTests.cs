using System.Text;
using System.Text.Json;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace IntegrityVault.Tests.Services;


// Define the test suite for the PDFService implementation.
public class PDFServiceTests
{
    private readonly Mock<ICryptoService> _mockCrypto;
    private readonly PDFService _service;


    public PDFServiceTests()
    {
        _mockCrypto = new Mock<ICryptoService>();
        _service = new PDFService(_mockCrypto.Object);
        QuestPDF.Settings.License = LicenseType.Community;
    }



    #region PDF Generation Tests

    [Fact]
    public void GeneratePDF_ShouldReturnValidByteArray_WhenDataIsProvided()
    {
        // Arrange.
        var data = CreateSampleData();
        byte[] encryptedJson = [1, 2, 3, 4, 5];

        // Act.
        var pdfBytes = _service.GeneratePDF(data, encryptedJson);

        // Assert.
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);

        // Basic PDF header check: %PDF-
        var header = Encoding.UTF8.GetString(pdfBytes[..5]);
        Assert.Equal("%PDF-", header);
    }

    #endregion



    #region PDF Extraction and Tamper-Evidence Tests

    [Fact]
    public void ExtractJsonFromPdf_ShouldReturnData_WhenMarkersArePresent()
    {
        // Arrange: Generate a real PDF first to extract from.
        var originalData = CreateSampleData();
        var jsonString = JsonSerializer.Serialize(originalData);
        byte[] encryptedBytes = Encoding.UTF8.GetBytes("EncryptedPayload");

        _mockCrypto.Setup(c => c.Decrypt(encryptedBytes)).Returns(jsonString);

        // Generate the PDF that contains the hidden markers.
        var pdfBytes = _service.GeneratePDF(originalData, encryptedBytes);

        // Act.
        var extractedData = _service.ExtractJsonFromPdf(pdfBytes);

        // Assert.
        Assert.NotNull(extractedData);
        Assert.Equal(originalData.PatientLastName, extractedData.PatientLastName);
        Assert.Equal(originalData.EpisodeID, extractedData.EpisodeID);
        _mockCrypto.Verify(c => c.Decrypt(It.IsAny<byte[]>()), Times.Once);
    }


    [Fact]
    public async Task ExtractJsonFromPdf_ShouldThrow_WhenMarkersAreMissing()
    {
        // Arrange: valid PDF, but no markers
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Content().Text("No markers here");
            });
        }).GeneratePdf();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Task.Run(() => _service.ExtractJsonFromPdf(pdfBytes)));

        Assert.Contains("Embedded JSON markers not found", ex.Message);
    }


    [Fact]
    public void ExtractJsonFromPdf_ShouldThrow_WhenJsonIsInvalid()
    {
        // Arrange
        var data = CreateSampleData();
        byte[] encryptedBytes = Encoding.UTF8.GetBytes("EncryptedPayload");

        // Generate PDF WITH markers
        var pdfBytes = _service.GeneratePDF(data, encryptedBytes);

        // Mock decrypt to return invalid JSON
        _mockCrypto.Setup(c => c.Decrypt(It.IsAny<byte[]>()))
            .Returns("Not-JSON-String");

        // Act & Assert
        Assert.Throws<JsonException>(() => _service.ExtractJsonFromPdf(pdfBytes));
    }

    #endregion



    #region Helper Methods

    // Helper to generate a valid DTO for testing.
    private static MedicalRecordPdfDataDTO CreateSampleData()
    {
        return new MedicalRecordPdfDataDTO
        {
            EpisodeID = 101,
            PatientID = 50,
            PatientFirstName = "John",
            PatientLastName = "Doe",
            PatientGender = PatientGender.Male,
            PatientDOB = new DateOnly(1990, 1, 1),
            DoctorID = 5,
            DoctorFirstName = "Jane",
            DoctorLastName = "Smith",
            DoctorSpecialy = DoctorSpecialty.Cardiology,
            HospitalID = 1,
            HospitalName = "Central Health",
            VisitDate = new DateOnly(2023, 10, 10),
            ChiefComplaint = "Chest Pain",
            Diagnosis = "Angina",
            TreatmentPlan = "Rest and medication",
            DoctorNotes = "Patient stable.",
            FollowUpInstructions = "See you in 2 weeks.",
            Version = 1
        };
    }

    #endregion
}