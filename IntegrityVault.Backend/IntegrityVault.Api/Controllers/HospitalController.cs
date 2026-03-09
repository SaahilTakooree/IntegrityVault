// Import dependencies needed to create the hospital controller.
using Microsoft.AspNetCore.Mvc; // Import ASP.Net Core MVC library for building APIs.
using IntegrityVault.Service.Interfaces; // Import the interface for the hospital service layer.
using IntegrityVault.Common.DTOs; // Import the DTOs for hospital.
using Microsoft.AspNetCore.Authorization; // Import ASP.NET Core for enabling authorisation.


// Declaring the namespace where this controller belongs.
namespace IntegrityVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Specifies the route pattern for API endpoints.
    public class HospitalController(IHospitalService _hospitalService) : ControllerBase // Define the HospitalController and injecting IHospitalService via the constructor.
    {
        // Specifies that this method will handle HTTP Get requests.
        [HttpGet]
        public async Task<IActionResult> GetAllHospital() // Method for get the complete list of all hospital.
        {
            try
            {
                var result = await _hospitalService.GetAllHospitalsAsync(); // Calling the GetAllHospitalsAsync method from the injected service to get a list of all hospitals.
                return Ok(result); // Returning a 200 OK response with the list of hospitals.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid get all hospitals: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Get requests but with the primary key.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHospitalById(int id) // Method for get a specific hospital.
        {
            try
            {
                var result = await _hospitalService.GetHospitalByIdAsync(id); // Calling the GetHospitalByIdAsync method from the injected service to get a specific hospital.

                // Return a 404 if a hosptal with a given ID does not exist.
                if (result is null)
                {
                    return NotFound($"Hospital with ID {id} was not found.");
                }

                // Return ok with the role-specific DTO.
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid get specific hospital: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost]
        public async Task<IActionResult> CreateHospital([FromBody] CreateHospitalDTO createHospitalDTO) // Method for creating a hopsital, accepting a CreateHospitalDTO object from the request body.
        {
            try
            {
                var result = await _hospitalService.CreateHospitalAsync(createHospitalDTO);
                return Ok(result); // Returning a 200 OK response with the result of hospital creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid hospital creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateHospital(int id, [FromBody] UpdateHospitalDTO updateHospitalDTO) // Method for update a hospital, accepting a UpdateHospitalDTO object from the request body.
        {
            try
            {
                var result = await _hospitalService.UpdateHospitalAsync(id, updateHospitalDTO); // Calling the UpdateHospitalAsync method from the injected service to update a. hospital info
                return Ok(result); // Returning a 200 OK response with the result of hospital updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid hospital update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Delete requests.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHospital(int id) // Method for deleting a specific hospital.
        {
            try
            {
                var result = await _hospitalService.DeleteHospitalAsync(id); // Calling the DeleteHospitalAsync method from the injected service to delete a specific hospital.
                return Ok(result); // Returning a 200 OK response with the result of deletion of specific hospital.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid hospital delete operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }
    }
}