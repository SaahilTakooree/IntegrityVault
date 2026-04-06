using System.Net;
using System.Text.Json;
using Moq.Protected;


namespace IntegrityVault.Tests.Services;


// Define the test suite for the IPFSService implementation.
public class IPFSServiceTests
{
    private readonly Mock<ICryptoService> _mockCrypto;
    private readonly Mock<IHttpClientFactory> _mockHttpFactory;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly IPFSService _service;


    public IPFSServiceTests()
    {
        _mockCrypto = new Mock<ICryptoService>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHttpFactory = new Mock<IHttpClientFactory>();

        // Setup the HttpClientFactory to return a client using our mocked handler.
        var client = new HttpClient(_mockHandler.Object);
        _mockHttpFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        _service = new IPFSService(_mockHttpFactory.Object, _mockCrypto.Object);
    }



    #region Upload (AddFileAsync) Tests

    [Fact]
    public async Task AddFileAsync_ShouldReturnCID_WhenAtLeastOneNodeSucceeds()
    {
        // Arrange.
        byte[] inputBytes = [1, 2, 3];
        byte[] encryptedBytes = [9, 8, 7];
        string expectedCid = "QmTestCid123";
        string ipfsResponse = JsonSerializer.Serialize(new { Hash = expectedCid });

        _mockCrypto.Setup(c => c.Encrypt(It.IsAny<string>())).Returns(encryptedBytes);

        // Simulate the first node failing and the second node succeeding.
        _mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) // Node 1 fails.
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) // Node 2 succeeds.
            {
                Content = new StringContent(ipfsResponse)
            });

        // Act.
        var result = await _service.AddFileAsync(inputBytes);

        // Assert.
        Assert.Equal(expectedCid, result);
    }


    [Fact]
    public async Task AddFileAsync_ShouldThrow_WhenAllNodesFail()
    {
        // Arrange.
        _mockCrypto.Setup(c => c.Encrypt(It.IsAny<string>())).Returns([1, 1, 1]);

        // All nodes return 500 Internal Server Error.
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act & Assert.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddFileAsync([0]));
        Assert.Contains("All IPFS nodes are unreachable", ex.Message);
    }

    #endregion



    #region Download (GetFileAsync) Tests

    [Fact]
    public async Task GetFileAsync_ShouldReturnOriginalBytes_AfterDecryption()
    {
        // Arrange.
        string cid = "QmDownload";
        byte[] encryptedFromIpfs = [5, 5, 5];
        string originalBase64 = Convert.ToBase64String([10, 20, 30]);

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(encryptedFromIpfs)
            });

        _mockCrypto.Setup(c => c.Decrypt(encryptedFromIpfs)).Returns(originalBase64);

        // Act.
        var result = await _service.GetFileAsync(cid);

        // Assert.
        Assert.Equal([10, 20, 30], result);
    }

    #endregion



    #region CID Only (GetCIDOnlyAsync) Tests

    [Fact]
    public async Task GetCIDOnlyAsync_ShouldRequestWithOnlyHashParameter()
    {
        // Arrange.
        string expectedCid = "QmHashOnly";
        string ipfsResponse = JsonSerializer.Serialize(new { Hash = expectedCid });

        _mockCrypto.Setup(c => c.Encrypt(It.IsAny<string>())).Returns([0]);

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Query.Contains("only-hash=true")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ipfsResponse)
            });

        // Act.
        var result = await _service.GetCIDOnlyAsync([1]);

        // Assert.
        Assert.Equal(expectedCid, result);
    }

    #endregion



    #region JSON Parsing Edge Cases

    [Fact]
    public async Task AddFileAsync_ShouldThrow_WhenResponseMissingHashField()
    {
        // Arrange: Invalid JSON structure from IPFS.
        string invalidResponse = JsonSerializer.Serialize(new { NotAHash = "oops" });
        _mockCrypto.Setup(c => c.Encrypt(It.IsAny<string>())).Returns([0]);

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(invalidResponse)
            });

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddFileAsync([1]));
    }

    #endregion
}