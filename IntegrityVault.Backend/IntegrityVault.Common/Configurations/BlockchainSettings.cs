// Declaring the namespace for the common configuration.
namespace IntegrityVault.Common.Configurations
{
    // Define the blockchain setting.
    public class BlockchainSettings
    {
        public required string RPC_URL { get; set; }
        public required string ContractAddress { get; set; }
        public required string SuperAdminWalletAddress { get; set; }
    }
}
