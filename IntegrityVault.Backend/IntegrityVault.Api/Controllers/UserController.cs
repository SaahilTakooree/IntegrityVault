// Import dependencies needed to create the user controller.
using Microsoft.AspNetCore.Mvc; // Import ASP.Net Core MVC library for building APIs.
using IntegrityVault.Service.Interfaces; // Import the interface for the user service layer.
using IntegrityVault.Common.DTOs; // Import the DTOs for User.


// Declaring the namespace where this controller belongs.
namespace IntegrityVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Specifies the route pattern for API endpoints.
    public class UserController(IUserService _userService) : ControllerBase // Define the UserController and injecting IUserService via the constructor.
    {
        // Specifies that this method will handle HTTP Get requests.
        [HttpGet]
        public async Task<IActionResult> GetAllUsers() // Method for get the complete list of all users.
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(); // Calling the GetAllUserAsync method from the injected service to get a list of all user.
                return Ok(result); // Returning a 200 OK response with the list of user DTOs.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid get all user: {ex.Message}."); // Logging the exception message to the console for debugging.
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
        public async Task<IActionResult> GetUserById(int id) // Method for get a specific user.
        {
            try
            {
                var result = await _userService.GetUserByIdAsync(id); // Calling the GetUserByIdAsync method from the injected service to get a specific user.

                // Return a 404 if a user with a given ID does not exist.
                if (result is null)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                // Return ok with the role-specific DTO.
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid get specific user: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("doctor")]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDTO createDoctorDTO) // Method for creating a doctor, accepting a CreateDoctorDTO object from the request body.
        {
            try
            {
                var result = await _userService.CreateDoctorAsync(createDoctorDTO);
                return Ok(result); // Returning a 200 OK response with the result of doctor creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid doctor creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("patient")]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDTO createPatientDTO) // Method for creating a patient, accepting a CreatePatientDTO object from the request body.
        {
            try
            {
                var result = await _userService.CreatePatientAsync(createPatientDTO);
                return Ok(result); // Returning a 200 OK response with the result of patient creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid patient creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO createAdminDTO) // Method for creating an admin, accepting a CreateAdminDTO object from the request body.
        {
            try
            {
                var result = await _userService.CreateAdminAsync(createAdminDTO);
                return Ok(result); // Returning a 200 OK response with the result of admin creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid admin creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("externalprovider")]
        public async Task<IActionResult> CreateExternalProvider([FromBody] CreateExternalProviderDTO createExternalProviderDTO) // Method for creating an external provider, accepting a CreateExternalProviderDTO object from the request body.
        {
            try
            {
                var result = await _userService.CreateExternalProviderAsync(createExternalProviderDTO);
                return Ok(result); // Returning a 200 OK response with the result of external provider creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid external provider creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP POST requests.
        [HttpPost("superadmin")]
        public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminDTO createSuperAdminDTO) // Method for creating a super admin, accepting a CreateSuperAdminDTO object from the request body.
        {
            try
            {
                var result = await _userService.CreateSuperAdminAsync(createSuperAdminDTO);
                return Ok(result); // Returning a 200 OK response with the result of super admin creation.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid super admin creating operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("doctor/{id:int}")]
        public async Task<IActionResult> UpdateDoctor (int id, [FromBody] UpdateDoctorDTO updateDoctorDTO) // Method for update a doctor, accepting a UpdateDoctorDTO object from the request body.
        {
            try
            {
                var result = await _userService.UpdateDoctorAsync(id, updateDoctorDTO); // Calling the UpdateDoctorAsync method from the injected service to update a doctor.
                return Ok(result); // Returning a 200 OK response with the result of doctor updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid doctor update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("patient/{id:int}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDTO updatePatientDTO) // Method for update a patient, accepting a UpdatePatientDTO object from the request body.
        {
            try
            {
                var result = await _userService.UpdatePatientAsync(id, updatePatientDTO); // Calling the UpdatePatientAsync method from the injected service to update a patient.
                return Ok(result); // Returning a 200 OK response with the result of patient updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid patient update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("admin/{id:int}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateAdminDTO updateAdminDTO) // Method for update an admin, accepting a UpdateAdminDTO object from the request body.
        {
            try
            {
                var result = await _userService.UpdateAdminAsync(id, updateAdminDTO); // Calling the UpdateAdminAsync method from the injected service to update an admin.
                return Ok(result); // Returning a 200 OK response with the result of admin updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid admin update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("externalprovider/{id:int}")]
        public async Task<IActionResult> UpdateExternalProvider(int id, [FromBody] UpdateExternalProviderDTO updateExternalProviderDTO) // Method for update an external provider, accepting a UpdateExternalProviderDTO object from the request body.
        {
            try
            {
                var result = await _userService.UpdateExternalProviderAsync(id, updateExternalProviderDTO); // Calling the UpdateExternalProviderAsync method from the injected service to update an external provider.
                return Ok(result); // Returning a 200 OK response with the result of externalprovider updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid external provider update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex) // Catching any general exception that may occur.
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Specifies that this method will handle HTTP Put requests.
        [HttpPatch("superadmin/{id:int}")]
        public async Task<IActionResult> UpdateSuperAdmin(int id, [FromBody] UpdateSuperAdminDTO updateSuperAdminDTO) // Method for update a super admin, accepting a UpdateSuperAdminDTO object from the request body.
        {
            try
            {
                var result = await _userService.UpdateSuperAdminAsync(id, updateSuperAdminDTO); // Calling the UpdateSuperAdminAsync method from the injected service to update a super admin.
                return Ok(result); // Returning a 200 OK response with the result of patient updates.
            }
            catch (InvalidOperationException ex) // Catching an InvalidOperationException if thrown.
            {
                Console.WriteLine($"Invalid super admin update operation: {ex.Message}."); // Logging the exception message to the console for debugging.
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
        public async Task<IActionResult> DeleteUser(int id) // Method for deleting a specific users.
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id); // Calling the DeleteUserAsync method from the injected service to delete a specific user.
                return Ok(result); // Returning a 200 OK response with the result of deletion of specific user.
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid user delete operation: {ex.Message}."); // Logging the exception message to the console for debugging.
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