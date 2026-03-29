// Import dependencies.
using IntegrityVault.Common.DTOs; // Importing the data transfer objects (DTOs) used for PDF creation.


// Declare the namespace for the service interfaces.
namespace IntegrityVault.Service.Interfaces
{
    // Define the IPDFService interface, which will implement the pdf service.
    public interface IPDFService
    {
        byte[] GeneratePDF(MedicalRecordPdfDataDTO data, byte[] encryptedJSON); // Method to Genereate a PDF file.
        MedicalRecordPdfDataDTO ExtractJsonFromPdf(byte[] pdfBytes); // Method to extract the embedded json in the PDF.
    }
}
