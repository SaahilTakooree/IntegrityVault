// Import dependencies.
using Nethereum.ABI.FunctionEncoding.Attributes; // Provides attributes to map smart contract function outputs to C# DTOs.
using System.Numerics; // Provides BigInteger type for handling large integer values used in blockchain.


// Defines the DTOs namespace for the IntegrityVault system.
namespace IntegrityVault.Common.DTOs
{
    // Maps the tuple returned by getRecord and getLatestRecord.
    [FunctionOutput]
    public class RecordEntryOutput : IFunctionOutputDTO
    {
        [Parameter("uint256", "episodeID", 1)]
        public BigInteger EpisodeID { get; set; }

        [Parameter("uint256", "ver", 2)]
        public BigInteger Version { get; set; }

        [Parameter("bytes32", "contentHash", 3)]
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();

        [Parameter("bytes32", "versionHash", 4)]
        public byte[] VersionHash { get; set; } = Array.Empty<byte>();
        [Parameter("string", "ipfsCID", 5)]

        public string IpfsCID { get; set; } = string.Empty;
        [Parameter("uint256", "timestamp", 6)]
        public BigInteger Timestamp { get; set; }

        [Parameter("address", "hospitalSigner", 7)]
        public string HospitalSigner { get; set; } = string.Empty;
    }
}