// Import dependencies.
using System.Text; // For string encoding and StringBuilder.
using IntegrityVault.Common.DTOs; // Data Transfer Objects used in the service.
using IntegrityVault.Service.Interfaces; // Import the interface for PDF service.
using System.Text.Json; // For JSON serialisation and deserialisation
using UglyToad.PdfPig; // For reading and extracting text from PDFs
using QuestPDF.Fluent; // Fluent API for creating PDFs.
using QuestPDF.Helpers; // Helper classes like Colors, and measurements


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Implementation of IPDFService for generating medical record PDFs.
    public class PDFService(ICryptoService _cryptoService) : IPDFService
    {
        // Brand colors used in PDF design.
        private static readonly string BrandTeal = "#0d9488"; 
        private static readonly string BrandLight = "#e6f7f6";
        private static readonly string TextDark = "#1a1a1a";
        private static readonly string TextMuted = "#6b7280";

        // Main function to generate the PDF from provided data and encrypted JSON.
        public byte[] GeneratePDF(MedicalRecordPdfDataDTO data, byte[] encryptedJSON)
        {
            // Memory stream to hold PDF bytes.
            var stream = new MemoryStream();

            // Create a PDF document using QuestPDF fluent API.
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Page setup.
                    page.Size(PageSizes.A4); // Set the page size to a standard A4.
                    page.Margin(0);
                    page.DefaultTextStyle(t =>
                        t.FontFamily("Helvetica").FontSize(11).FontColor(TextDark)); // Default font style.

                    // Header section of the PDF.
                    page.Header()
                        .Background(BrandTeal) // Teal background for header.
                        .Padding(32) // Padding around content.
                        .Row(row =>
                        {
                            // Left side of header.
                            row.RelativeItem().Column(col =>
                            {
                                col.Item()
                                    .Text("Medical Record") // PDF title.
                                    .FontSize(22)
                                    .FontColor(Colors.White)
                                    .Bold();

                                col.Item().PaddingTop(4)
                                    .Text($"{data.HospitalName}") // Hospital name.
                                    .FontSize(12)
                                    .FontColor("#b2e4e1"); // Slightly lighter text.
                            });

                            // Right side of header.
                            row.AutoItem()
                                .AlignBottom()
                                .Border(0.5f).BorderColor("#b2e4e1")
                                .PaddingVertical(6).PaddingHorizontal(12)
                                .Text($"v{data.Version}  ·  Confidential")
                                .FontSize(11)
                                .FontColor(Colors.White);
                        });

                    // Body section of the PDF.
                    page.Content()
                        .PaddingHorizontal(40)
                        .PaddingTop(24)
                        .Column(col =>
                        {
                            col.Spacing(0);

                            // Add patient and doctor metadata table.
                            BuildMetaGrid(col, data);

                            // Divider line.
                            col.Item().PaddingVertical(16)
                                .LineHorizontal(0.5f)
                                .LineColor(Colors.Grey.Lighten2);

                            // Add clinical sections.
                            BuildSection(col, "Chief Complaint", data.ChiefComplaint);
                            BuildSection(col, "Diagnosis", data.Diagnosis);
                            BuildSection(col, "Treatment Plan", data.TreatmentPlan);
                            BuildSection(col, "Doctor Notes", data.DoctorNotes);
                            BuildSection(col, "Follow-up Instructions", data.FollowUpInstructions);

                            col.Item().PaddingTop(16);

                            // Add hidden Base64-encoded JSON for tamper-evidence.
                            col.Item()
                                .Text($"IV_JSON_START::{Convert.ToBase64String(encryptedJSON)}::IV_JSON_END")
                                .FontSize(1)
                                .FontColor(Colors.White);
                        });

                    // Footer section.
                    page.Footer()
                        .BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .PaddingHorizontal(40)
                        .PaddingVertical(10)
                        .Row(row =>
                        {
                            // Left side of footer.
                            row.RelativeItem()
                                .Text("IntegrityVault  ·  Encrypted & tamper-evident")
                                .FontSize(9)
                                .FontColor(TextMuted);

                            // Right side: page number.
                            row.AutoItem()
                                .AlignRight()
                                .Text(x =>
                                {
                                    x.CurrentPageNumber();
                                });
                        });
                });
            }).GeneratePdf(stream);

            // Return PDF as byte array.
            return stream.ToArray();
        }


        // Helper to build metadata table for patient and doctor info.
        private static void BuildMetaGrid(ColumnDescriptor col, MedicalRecordPdfDataDTO data)
        {
            var patientName = FormatName(data.PatientFirstName, data.PatientMiddleName, data.PatientLastName);
            var doctorName = $"Dr. {FormatName(data.DoctorFirstName, data.DoctorMiddleName, data.DoctorLastName)}";

            var gender = data.PatientGender.ToString();
            var specialty = data.DoctorSpecialy.ToString();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                MetaCell(table, "Patient", patientName);
                MetaCell(table, "Visit Date", data.VisitDate.ToString("dd-MM-yyyy"));
                MetaCell(table, "Date of Birth", data.PatientDOB.ToString("dd-MM-yyyy"));
                MetaCell(table, "Gender", gender);
                MetaCell(table, "Attending", doctorName);
                MetaCell(table, "Specialty", specialty);
            });
        }


        // Helper to generate one metadata cell.
        private static void MetaCell(TableDescriptor table, string label, string value)
        {
            table.Cell()
                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(8).PaddingHorizontal(4)
                .Column(c =>
                {
                    c.Item()
                        .Text(label.ToUpper())
                        .FontSize(9)
                        .FontColor(TextMuted)
                        .LetterSpacing(0.04f);

                    c.Item().PaddingTop(2)
                        .Text(value ?? "—")
                        .FontSize(12)
                        .Bold();
                });
        }


        // Helper to build each clinical section.
        private static void BuildSection(ColumnDescriptor col, string title, string? value)
        {
            col.Item().PaddingBottom(14).Column(c =>
            {
                // Section title with left border.
                c.Item()
                    .BorderLeft(2).BorderColor(BrandTeal)
                    .PaddingLeft(8)
                    .Text(title.ToUpper())
                    .FontSize(9)
                    .FontColor(BrandTeal)
                    .LetterSpacing(0.06f);

                // Section content block.
                c.Item()
                    .PaddingTop(4)
                    .Background(BrandLight)
                    .PaddingVertical(10).PaddingHorizontal(12)
                    .Text(value ?? "—")
                    .FontSize(11)
                    .LineHeight(1.6f);
            });
        }


        // Method to extract the embedded json in the PDF.
        public MedicalRecordPdfDataDTO ExtractJsonFromPdf(byte[] pdfBytes)
        {
            // Load the PDF from byte array into a readable document.
            using var stream = new MemoryStream(pdfBytes);
            using var document = PdfDocument.Open(stream);

            var fullText = new StringBuilder();

            // Extract all text content from each page of the PDF.
            foreach (var page in document.GetPages())
            {
                fullText.Append(page.Text);
            }

            var text = fullText.ToString();

            // Define markers used to safely locate the embedded encrypted JSON.
            var startMarker = "IV_JSON_START::";
            var endMarker = "::IV_JSON_END";

            // Find positions of the markers in the extracted text.
            var startIndex = text.IndexOf(startMarker);
            var endIndex = text.IndexOf(endMarker);

            // Validate that both markers exist and are in the correct order.
            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                throw new InvalidOperationException("Embedded JSON markers not found in PDF.");
            }

            // Extract only the content between the markers.
            var rawText = text[(startIndex + startMarker.Length)..endIndex];

            // Validate that the extracted content is a valid Base64 string.
            var matches = System.Text.RegularExpressions.Regex.Matches(rawText, @"^[A-Za-z0-9+/=]+$");
            if (matches.Count == 0)
            {
                throw new InvalidOperationException("Embedded JSON is not a valid Base64 string.");
            }

            var base64 = matches[0].Value;

            // Decode the data Decrypt tthe data deserialise the data.
            var encryptedBytes = Convert.FromBase64String(base64);
            var json = _cryptoService.Decrypt(encryptedBytes);
            var data = JsonSerializer.Deserialize<MedicalRecordPdfDataDTO>(json);

            return data ?? throw new InvalidOperationException("Failed to deserialise medical record.");
        }



        // Helper to concatenate names safely, ignoring null or empty strings.
        private static string FormatName(string? first, string? middle, string? last) =>
            string.Join(" ", new[] { first, middle, last }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}