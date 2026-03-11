// Using directive to include the IntegrityVault.Common.Enums namespace.
using IntegrityVault.Common.Enums;


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO for the based field shared by all user types returned when reading any user.
    public class UserDTO
    {
        public int ID { get; set; }
        public string Username { get; set; } = string.Empty!;
        public string Email { get; set; } = string.Empty!;
        public string Password { get; set; } = string.Empty!;
        public UserRole Role { get; set; }
        public DateTime JoinDate { get; set; }
        public int? HospitalID { get; set; }
    }

    // DTO return when reading a doctor.
    public class DoctorDTO : UserDTO
    {
        public string FirstName { get; set; } = string.Empty!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty!;
        public DoctorSpecialty Specialty { get; set; }
    }


    // DTO return when reading a patient.
    public class PatientDTO : UserDTO
    {
        public string FirstName { get; set; } = string.Empty!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty!;
        public DateOnly DOB { get; set; }
        public PatientGender Gender { get; set; }
    }


    // DTO return when reading an admin.
    public class AdminDTO : UserDTO
    {
    }

    // DTO return when reading an external provider
    public class ExternalProviderDTO : UserDTO
    {
        public int BelongsToID { get; set; }
    }

    // DTO return when reading a super
    public class SuperAdminDTO : UserDTO
    {
    }


    // Base DTO for creating shared user fields.
    public class CreateUserDTO
        {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int? HospitalID { get; set; }
    }


    // DTO for creating a doctor
    public class CreateDoctorDTO : CreateUserDTO
    {
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public required DoctorSpecialty Specialty { get; set; }
    }


    // DTO for creating a patient.
    public class CreatePatientDTO : CreateUserDTO
    {
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public DateOnly DOB { get; set; }
        public PatientGender Gender { get; set; }
    }


    // DTO for creating an admin.
    public class CreateAdminDTO : CreateUserDTO
    {
    }


    // DTO for creating an external provider.
    public class CreateExternalProviderDTO : CreateUserDTO
    {
        public required int BelongsToID { get; set; }
    }

    // DTO for creating an super admin.
    public class CreateSuperAdminDTO : CreateUserDTO
    {
    }


    // Base DTO for updating shared user fields.
    public class UpdateUserDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? HospitalID { get; set; }
    }


    // DTO for updating a doctor
    public class UpdateDoctorDTO : UpdateUserDTO
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DoctorSpecialty? Specialty { get; set; }
    }


    // DTO for update a patient.
    public class UpdatePatientDTO : UpdateUserDTO
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DOB { get; set; }
        public PatientGender? Gender { get; set; }
    }


    // DTO for updating an admin.
    public class UpdateAdminDTO : UpdateUserDTO
    {
    }


    // DTO for updating an external provider.
    public class UpdateExternalProviderDTO : UpdateUserDTO
    {
        public int? BelongsToID { get; set; }
    }

    // DTO for updating an admin.
    public class UpdateSuperAdminDTO : UpdateUserDTO
    {
    }
}