// Import dependencies.
using Nethereum.ABI.FunctionEncoding.Attributes; // Provides attributes to map smart contract function outputs to C# DTOs.
using Nethereum.Contracts; // Provides base types and interfaces for interacting with smart contracts.
using System.Numerics; // Provides BigInteger type for handling large integer values used in blockchain.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // Define function message for anchoring a new medical record on-chain.
    [Function("registerRecord")]
    public class RegisterRecordFunction : FunctionMessage
    {
        [Parameter("uint256", "recordID", 1)]
        public BigInteger RecordID { get; set; }

        [Parameter("uint256", "episodeID", 2)]
        public BigInteger EpisodeID { get; set; }

        [Parameter("bytes32", "contentHash", 3)]
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();

        [Parameter("bytes32", "versionHash", 4)]
        public byte[] VersionHash { get; set; } = Array.Empty<byte>();

        [Parameter("string", "ipfsCID", 5)]
        public string IpfsCID { get; set; } = string.Empty;
    }


    // Define function message for anchoring an updated medical record on-chain.
    [Function("updateRecord")]
    public class UpdateRecordFunction : FunctionMessage
    {
        [Parameter("uint256", "recordID", 1)]
        public BigInteger RecordID { get; set; }

        [Parameter("uint256", "expectedCurrentVersion", 2)]
        public BigInteger ExpectedCurrentVersion { get; set; }

        [Parameter("bytes32", "newContentHash", 3)]
        public byte[] NewContentHash { get; set; } = Array.Empty<byte>();

        [Parameter("bytes32", "newVersionHash", 4)]
        public byte[] NewVersionHash { get; set; } = Array.Empty<byte>();

        [Parameter("string", "newIpfsCID", 5)]
        public string NewIpfsCID { get; set; } = string.Empty;
    }
}
