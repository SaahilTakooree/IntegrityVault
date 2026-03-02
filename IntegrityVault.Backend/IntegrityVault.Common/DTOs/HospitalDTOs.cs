// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // DTO for hospital.
    public class HospitalDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string WalletAddress { get; set; }

    }


    // DTO to create a hospital.
    public class CreateHospitalDTO
    {
        public required string Name { get; set; }
        public required string WalletAddress { get; set; }
    }


    // DTO to update a hospital.
    public class UpdateHospitalDTO
    {
        public string? Name { get; set; }
        public string? WalletAddress { get; set; }
    }
}
