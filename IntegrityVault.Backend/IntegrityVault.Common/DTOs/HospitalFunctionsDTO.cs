// Import dependencies.
using Nethereum.ABI.FunctionEncoding.Attributes; // Provides attributes to map smart contract function outputs to C# DTOs.
using Nethereum.Contracts; // Provides base types and interfaces for interacting with smart contracts.
using System.Numerics; // Provides BigInteger type for handling large integer values used in blockchain.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // Define function message for adding a hospital on-chain.
    [Function("addHospital")]
    public class AddHospitalFunction : FunctionMessage
    {
        [Parameter("uint256", "hospitalId", 1)]
        public BigInteger HospitalId { get; set; }

        [Parameter("address", "wallet", 2)]
        public string Wallet { get; set; } = string.Empty;
    }


    // Define function message for updating a hospital's wallet on-chain.
    [Function("updateHospitalWallet")]
    public class UpdateHospitalWalletFunction : FunctionMessage
    {
        [Parameter("uint256", "hospitalId", 1)]
        public BigInteger HospitalId { get; set; }

        [Parameter("address", "newWallet", 2)]
        public string NewWallet { get; set; } = string.Empty;
    }


    // Define function message for removing a hospital from the blockchain.
    [Function("removeHospital")]
    public class RemoveHospitalFunction : FunctionMessage
    {
        [Parameter("uint256", "hospitalId", 1)]
        public BigInteger HospitalId { get; set; }
    }
}