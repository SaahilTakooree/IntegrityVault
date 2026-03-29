// Import dependencies.
using System.Net.Http.Headers; // For setting HTTP content headers.
using System.Text.Json; // For parsing JSON responses from IPFS nodes.
using IntegrityVault.Service.Interfaces;  // Import the interface for the IPFS service.


// Declaring the namespace where this service implementation resides.
namespace IntegrityVault.Service.Implementations
{
    // Define the IPFSService class and injecting the ICryptoService and IHttpClientFactory.
    public class IPFSService(IHttpClientFactory _httpClientFactory, ICryptoService _cryptoService) : IIPFSService
    {
        // Define the IPFS nodes.
        private static readonly string[] _ipfsNodes =
        [
            "http://127.0.0.1:5002", // Node 1.
            "http://127.0.0.1:5003", // Node 2.
            "http://127.0.0.1:5004" // Node 3.
        ];


        // Method to encrypts and upload a file to IPFS. Return the CID.
        public async Task<string> AddFileAsync(byte[] fileBytes)
        {
            // Encrypt the file bytes as base64 string.
            var encryptedBytes = _cryptoService.Encrypt(Convert.ToBase64String(fileBytes));

            // Try uploading to each node until successful.
            foreach (var node in SelectNodes())
            {
                try
                {
                    return await UploadToNodeAsync(node, encryptedBytes);
                }
                catch (Exception)
                {
                }
            }

            throw new InvalidOperationException("All IPFS nodes are unreachable. Upload failed.");
        }


        // Method to retrieve and decrypts a file from IPFS by CID.
        public async Task<byte[]> GetFileAsync(string CID)
        {
            // Try uploading to each node until successful.
            foreach (var node in SelectNodes())
            {
                try
                {
                    var encryptedBytes = await DownloadFromNodeAsync(node, CID);

                    // Decrypt the bytes back to the original file.
                    var base64 = _cryptoService.Decrypt(encryptedBytes);
                    return Convert.FromBase64String(base64);
                }
                catch (Exception)
                {
                }
            }

            throw new InvalidOperationException($"CID '{CID}' could not be retrieved from any IPFS node.");
        }


        // Method to computes the CID of bytes without uploading the IPFS.
        public async Task<string> GetCIDOnlyAsync(byte[] fileBytes)
        {
            // Encrypt the file bytes as base64 string.
            var encryptedBytes = _cryptoService.Encrypt(Convert.ToBase64String(fileBytes));

            // Try uploading to each node until successful.
            foreach (var node in SelectNodes())
            {
                try
                {
                    // Only compute hash without storing.
                    return await UploadToNodeAsync(node, encryptedBytes, onlyHash: true);
                }
                catch (Exception)
                {
                }
            }

            throw new InvalidOperationException("All IPFS nodes are unreachable. CID computation failed.");
        }


        // Private method to random select nodes.
        private static IEnumerable<string> SelectNodes()
        {
            return _ipfsNodes.OrderBy(_ => Random.Shared.Next());
        }


        // Uploads encrypted bytes to a specific IPFS node.
        private async Task<string> UploadToNodeAsync(string nodeUrl, byte[] encryptedBytes, bool onlyHash = false)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{nodeUrl.TrimEnd('/')}/api/v0/add";
            if (onlyHash) url += "?only-hash=true";

            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(encryptedBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Add file content to multipart form data for IPFS API.
            content.Add(fileContent, "file", "file");

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // Parse the JSON response and extract the "Hash" field.
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("Hash", out var hashProp))
                throw new InvalidOperationException("IPFS response did not contain a Hash field.");

            return hashProp.GetString() ?? throw new InvalidOperationException("IPFS returned a null CID.");
        }


        // Downloads encrypted bytes from a specific IPFS node by CID.
        private async Task<byte[]> DownloadFromNodeAsync(string nodeUrl, string cid)
        {
            var client = _httpClientFactory.CreateClient();

            var url = $"{nodeUrl.TrimEnd('/')}/api/v0/cat?arg={cid}";
            var response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
