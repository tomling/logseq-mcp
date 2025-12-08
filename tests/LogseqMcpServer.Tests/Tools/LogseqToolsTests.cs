using System.Net;
using System.Text.Json;
using LogseqMcpServer.Configuration;
using LogseqMcpServer.Models;
using LogseqMcpServer.Services;
using LogseqMcpServer.Tools;
using Moq;
using Moq.Protected;

namespace LogseqMcpServer.Tests.Tools;

public class LogseqToolsTests
{
    private readonly LogseqConfiguration _testConfig;

    public LogseqToolsTests()
    {
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token");
        _testConfig = LogseqConfiguration.LoadFromEnvironment();
    }

    private LogseqHttpClient CreateMockClient(HttpResponseMessage response)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_testConfig.BaseUrl)
        };

        return new LogseqHttpClient(httpClient, _testConfig);
    }

    [Fact]
    public async Task CreatePage_WithInvalidJsonProperties_ReturnsErrorMessage()
    {
        // Arrange
        var client = CreateMockClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}")
        });

        // Act
        var result = await LogseqTools.CreatePage(client, "Test Page", "{ invalid json }");

        // Assert
        Assert.Contains("Invalid JSON", result);
    }

    [Fact]
    public async Task CreatePage_WithValidJsonProperties_CallsApiSuccessfully()
    {
        // Arrange
        var expectedPage = new PageEntity { Name = "Test Page", Uuid = "test-uuid" };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPage))
        };

        var client = CreateMockClient(response);
        var validProperties = "{\"tags\": [\"tag1\", \"tag2\"]}";

        // Act
        var result = await LogseqTools.CreatePage(client, "Test Page", validProperties);

        // Assert
        Assert.Contains("Successfully created", result);
        Assert.Contains("Test Page", result);
    }

    [Fact]
    public async Task AppendBlock_WithInvalidJsonProperties_ReturnsErrorMessage()
    {
        // Arrange
        var client = CreateMockClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}")
        });

        // Act
        var result = await LogseqTools.AppendBlock(client, "Test Page", "Content", "{ invalid json }");

        // Assert
        Assert.Contains("Invalid JSON", result);
    }

    [Fact]
    public async Task AppendBlock_WithValidJsonProperties_CallsApiSuccessfully()
    {
        // Arrange
        var expectedBlock = new BlockEntity { Content = "Test Content", Uuid = "block-uuid" };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedBlock))
        };

        var client = CreateMockClient(response);
        var validProperties = "{\"priority\": \"high\"}";

        // Act
        var result = await LogseqTools.AppendBlock(client, "Test Page", "Test Content", validProperties);

        // Assert
        Assert.Contains("Successfully appended", result);
    }

    [Fact]
    public async Task UpdateBlock_WithInvalidJsonProperties_ReturnsErrorMessage()
    {
        // Arrange
        var client = CreateMockClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}")
        });

        // Act
        var result = await LogseqTools.UpdateBlock(client, "block-uuid", "Content", "{ invalid json }");

        // Assert
        Assert.Contains("Invalid JSON", result);
    }

    [Fact]
    public async Task SearchContent_WithEmptyQuery_ReturnsErrorMessage()
    {
        // Arrange
        var client = CreateMockClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[]")
        });

        // Act
        var result = await LogseqTools.SearchContent(client, "", 10);

        // Assert
        Assert.Contains("Search query cannot be empty", result);
    }

    [Fact]
    public async Task SearchContent_WithWhitespaceQuery_ReturnsErrorMessage()
    {
        // Arrange
        var client = CreateMockClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[]")
        });

        // Act
        var result = await LogseqTools.SearchContent(client, "   ", 10);

        // Assert
        Assert.Contains("Search query cannot be empty", result);
    }

    [Fact]
    public async Task ListPages_WithNoPages_ReturnsAppropriateMessage()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[]")
        };

        var client = CreateMockClient(response);

        // Act
        var result = await LogseqTools.ListPages(client);

        // Assert
        Assert.Contains("No pages found", result);
    }

    [Fact]
    public async Task ListPages_WithMultiplePages_ReturnsFormattedList()
    {
        // Arrange
        var pages = new[]
        {
            new PageEntity { Name = "Page 1", Uuid = "uuid-1" },
            new PageEntity { Name = "Page 2", Uuid = "uuid-2" }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(pages))
        };

        var client = CreateMockClient(response);

        // Act
        var result = await LogseqTools.ListPages(client);

        // Assert
        Assert.Contains("Found 2 pages", result);
        Assert.Contains("Page 1", result);
        Assert.Contains("Page 2", result);
    }
}
