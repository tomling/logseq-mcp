using System.Net;
using System.Text.Json;
using LogseqMcpServer.Configuration;
using LogseqMcpServer.Models;
using LogseqMcpServer.Services;
using Moq;
using Moq.Protected;

namespace LogseqMcpServer.Tests.Services;

public class LogseqHttpClientTests
{
    private readonly LogseqConfiguration _testConfig;

    public LogseqHttpClientTests()
    {
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token");
        _testConfig = LogseqConfiguration.LoadFromEnvironment();
    }

    [Fact]
    public async Task CallApiAsync_WithSuccessfulResponse_ReturnsDeserializedResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var expectedPage = new PageEntity { Name = "Test Page", Uuid = "test-uuid-123" };
        var responseContent = JsonSerializer.Serialize(expectedPage);

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_testConfig.BaseUrl)
        };

        var logseqClient = new LogseqHttpClient(httpClient, _testConfig);

        // Act
        var result = await logseqClient.CallApiAsync<PageEntity>("logseq.Editor.createPage", default, "Test Page");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Page", result.Name);
        Assert.Equal("test-uuid-123", result.Uuid);
    }

    [Fact]
    public async Task CallApiAsync_WithUnauthorizedResponse_ThrowsLogseqApiException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_testConfig.BaseUrl)
        };

        var logseqClient = new LogseqHttpClient(httpClient, _testConfig);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LogseqApiException>(
            () => logseqClient.CallApiAsync<PageEntity>("logseq.Editor.createPage", default, "Test Page"));

        Assert.Contains("Authentication failed", exception.Message);
        Assert.Contains("LOGSEQ_AUTH_TOKEN", exception.Message);
    }

    [Fact]
    public async Task CallApiAsync_WithHttpRequestException_ThrowsLogseqApiException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_testConfig.BaseUrl)
        };

        var logseqClient = new LogseqHttpClient(httpClient, _testConfig);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LogseqApiException>(
            () => logseqClient.CallApiAsync<PageEntity>("logseq.Editor.createPage", default, "Test Page"));

        Assert.Contains("Cannot connect to Logseq", exception.Message);
    }

    [Fact]
    public async Task CallApiAsync_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException());

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_testConfig.BaseUrl)
        };

        var logseqClient = new LogseqHttpClient(httpClient, _testConfig);

        // Act & Assert
        await Assert.ThrowsAsync<LogseqApiException>(
            () => logseqClient.CallApiAsync<PageEntity>("logseq.Editor.createPage", cts.Token, "Test Page"));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LogseqHttpClient(null!, _testConfig));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LogseqHttpClient(httpClient, null!));
    }
}
