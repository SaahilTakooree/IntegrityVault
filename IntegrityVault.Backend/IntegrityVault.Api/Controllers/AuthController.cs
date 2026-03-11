// Import dependencies
using Microsoft.AspNetCore.Mvc; // Provides controller base classes and MVC attributes.
using Microsoft.IdentityModel.Tokens; // Provides security token functionality.
using IntegrityVault.Service.Interfaces; // Provides access to authentication service interface.
using System.IdentityModel.Tokens.Jwt; // Provides JWT token creation and handling.
using System.Security.Claims; // Provides claim-based identity functionality.
using System.Text; // Provides text encoding functionality.
using IntegrityVault.Common.DTOs; // Provides data transfer objects.
using IntegrityVault.Common.Entities; // Provide the user entity.


// Define the namespace for API controllers.
namespace IntegrityVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService _authService, IConfiguration _configuration) : ControllerBase // Inject authentication service and configuration.
    {

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthLoginDTO authLoginDTO) // Login endpoint that receives credentials.
        {
            try
            {
                // Call service to validate username/email and password.
                var user = await _authService.LoginAsync(authLoginDTO.UsernameOrEmail, authLoginDTO.Password, authLoginDTO.IpAddress);

                // Send authorised if credential are incorrect.
                if (user == null)
                    return Unauthorized("Invalid credentials");

                var token = CreateToken(user);

                // Return generated token and user information.
                return Ok(new { token });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid login: {ex.Message}."); // Logging the exception message to the console for debugging.
                return BadRequest(ex.Message); // Returning a 400 BadRequest with the exception message.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}."); // Logging the exception message to the console for debugging.
                return StatusCode(500, $"Internal server error: {ex.Message}."); // Returning a 500 Internal Server Error with the exception message.
            }
        }


        // Private method to create JWT token.
        private string CreateToken(User user)
        {
            // Create claims for the authenticated user.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.ID.ToString()),
                new(ClaimTypes.Role, user.Role.ToString()!),
                new("HospitalID", user.HospitalID.ToString()!),
                new("LastName", GetLastName(user))
            };

            // Create security key from JWT secret key.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)); // Generate symmetric security key.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Create signing credentials.
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            ); // Create JWT token with claims and expiration.

            // Convert JWT token to string format.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }


        // Method to get the last name from the subclass.
        private static string GetLastName(User user) => user switch
        {
            Doctor doctor => doctor.LastName ?? "",
            Patient patient => patient.LastName ?? "",
            _ => ""
        };
    }
}