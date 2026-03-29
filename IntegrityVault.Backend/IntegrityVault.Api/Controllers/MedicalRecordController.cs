// Import dependencies needed to create the medical record controller.
using Microsoft.AspNetCore.Mvc; // Import ASP.Net Core MVC library for building APIs.
using IntegrityVault.Service.Interfaces; // Import the interface for the medical record service layer.
using IntegrityVault.Common.DTOs; // Import the DTOs for medical record.
using Microsoft.AspNetCore.Authorization; // Import ASP.NET Core for enabling authorisation.


// Declaring the namespace where this controller belongs.
namespace IntegrityVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Specifies the route pattern for API endpoints.
    public class MedicalRecordController(IMedicalRecordService _medicalRecordService, IEpisodeService _episodeService) : ControllerBase // Define the MedicalRecordController and injecting IMedicalRecordService via the constructor.
    {

        // Specifies that this method will handle HTTP POST requests.
        [HttpPost]
        public async Task<IActionResult> CreateNewMedicalRecordAndEpisode([FromBody] CreateMedicalRecordDTO createMedicalRecordDTO) // Method for creating a new medical and an episode, accepting a CreateMedicalRecordDTO object from the request body.
        {
            try
            {
                var result = await _medicalRecordService.CreateMedicalRecordAndEpisodeAsync(createMedicalRecordDTO); // Method for creating a new medical for a episode, accepting a CreateMedicalRecordDTO object from the request body.
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid medical record creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests by ID.
        [HttpPost("episode/{episodeID:int}")]
        public async Task<IActionResult> AddMedicalRecordToEpisode(int episodeID, [FromBody] CreateMedicalRecordDTO createMedicalRecordDTO) // Method for creating a new medical for an existing episode, accepting a CreateMedicalRecordDTO object from the request body.
        {
            try
            {
                var result = await _medicalRecordService.AddMedicalRecordToEpisodeAsync(episodeID, createMedicalRecordDTO);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid medical record addition: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Patch requests.
        [HttpPatch("episode/{episodeID:int}/{medicalRecordID:int}")]
        public async Task<IActionResult> UpdateMedicalrecord(int medicalRecordID, int episodeID, [FromBody] CreateMedicalRecordDTO createMedicalRecordDTO) // Method for update a medical record, accepting a CreateMedicalRecordDTO object from the request body.
        {
            try
            {
                var result = await _medicalRecordService.PatchMedicalRecordAsync(medicalRecordID, episodeID, createMedicalRecordDTO);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid medical record updating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP GET requests.
        [HttpGet("patient/{patientID:int}/history")]
        public async Task<IActionResult> GetPatientMedicalHistory(int patientID) // Method that get patient full medcial history.
        {
            try
            {
                var history = await _medicalRecordService.GetPatientMedicalHistoryAsync(patientID);
                return Ok(history);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation getting patient history: {ex.Message}.");  // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP GET requests.
        [HttpGet("doctor/{doctorID:int}/history")]
        public async Task<IActionResult> GetDoctorMedicalHistory(int doctorID)
        {
            try
            {
                var history = await _medicalRecordService.GetDoctorMedicalHistoryAsync(doctorID);
                return Ok(history);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation getting doctor history: {ex.Message}.");  // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}.");  // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP PATCH requests.
        [HttpPatch("episode/{episodeID:int}/status")]
        public async Task<IActionResult> SetEpisodeStatus(int episodeID, [FromBody] int doctorID)
        {
            try
            {
                var result = await _episodeService.SetEpisodeStatusAsync(episodeID, doctorID); 
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation updating episode status: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 400 BadRequest with the exception message.
            }
        }


        // Specifies that this method will handle HTTP GET requests.
        [HttpGet("ipfs/{cid}/user/{userID:int}")]
        public async Task<IActionResult> GetMedicalRecordInformFromCID(string cid, int userID) // Method that retrieves a medical record from IPFS using CID and user ID.
        {
            try
            {
                var record = await _medicalRecordService.GetMedicalRecordInformationFromCIDAsync(cid, userID);

                return Ok(new
                {
                    record.ChiefComplaint,
                    record.Diagnosis,
                    record.TreatmentPlan,
                    record.DoctorNotes,
                    record.FollowUpInstructions
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation retrieving medical record from CID: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP GET requests.
        [HttpGet("ipfs/{cid}/user/{userID:int}/tamper-check")]
        public async Task<IActionResult> CheckIfMedicalRecordTampered(string cid, int userID) // Method that checks whether a medical record stored in IPFS has been tampered with.
        {
            try
            {
                var isTampered = await _medicalRecordService.IsMedicalRecordTamperedAsync(cid, userID);

                return Ok(isTampered); // Returning a boolean indicating whether the record has been tampered with.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid tamper check operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("pdf/tamper-check/user/{userID:int}")]
        public async Task<IActionResult> VerifyPdfTampering(int userID, IFormFile file) // Method that receives a PDF file and checks whether the medical record has been tampered with.
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file."); // Returning a 400 BadRequest if file is null or empty.
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var isTampered = await _medicalRecordService.VerifyPdfTamperingAsync(memoryStream.ToArray(), userID);

                return Ok(isTampered); // Returning a boolean indicating whether the PDF has been tampered with.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid PDF tamper verification operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        [HttpGet("ipfs/{cid}/user/{userID:int}/download")]
        public async Task<IActionResult> DownloadMedicalRecord(string cid, int userID)
        {
            try
            {
                var (pdfBytes, filename) = await _medicalRecordService.DownloadMedicalRecordAsync(cid, userID);

                return File(
                    pdfBytes,
                    "application/pdf",
                    $"{filename}.pdf"
                );
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation downloading medical record: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 400 BadRequest with the exception message.
            }
        }
    }
}