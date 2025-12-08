using LogseqMcpServer.Configuration;

namespace LogseqMcpServer.Tests.Configuration;

public class LogseqConfigurationTests
{
    [Fact]
    public void LoadFromEnvironment_WithMissingAuthToken_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            LogseqConfiguration.LoadFromEnvironment());

        Assert.Contains("LOGSEQ_AUTH_TOKEN", exception.Message);
    }

    [Fact]
    public void LoadFromEnvironment_WithEmptyAuthToken_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "   ");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            LogseqConfiguration.LoadFromEnvironment());

        Assert.Contains("LOGSEQ_AUTH_TOKEN", exception.Message);
    }

    [Fact]
    public void LoadFromEnvironment_WithValidAuthToken_ReturnsConfiguration()
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token-123");
        Environment.SetEnvironmentVariable("LOGSEQ_HOST", null);
        Environment.SetEnvironmentVariable("LOGSEQ_PORT", null);

        try
        {
            // Act
            var config = LogseqConfiguration.LoadFromEnvironment();

            // Assert
            Assert.Equal("test-token-123", config.AuthToken);
            Assert.Equal("host.docker.internal", config.Host);
            Assert.Equal(12315, config.Port);
            Assert.Equal("http://host.docker.internal:12315", config.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", null);
        }
    }

    [Fact]
    public void LoadFromEnvironment_WithCustomHostAndPort_ReturnsConfiguration()
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token-123");
        Environment.SetEnvironmentVariable("LOGSEQ_HOST", "localhost");
        Environment.SetEnvironmentVariable("LOGSEQ_PORT", "8080");

        try
        {
            // Act
            var config = LogseqConfiguration.LoadFromEnvironment();

            // Assert
            Assert.Equal("test-token-123", config.AuthToken);
            Assert.Equal("localhost", config.Host);
            Assert.Equal(8080, config.Port);
            Assert.Equal("http://localhost:8080", config.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", null);
            Environment.SetEnvironmentVariable("LOGSEQ_HOST", null);
            Environment.SetEnvironmentVariable("LOGSEQ_PORT", null);
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("65536")]
    [InlineData("99999")]
    [InlineData("not-a-number")]
    public void LoadFromEnvironment_WithInvalidPort_ThrowsInvalidOperationException(string invalidPort)
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token-123");
        Environment.SetEnvironmentVariable("LOGSEQ_PORT", invalidPort);

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                LogseqConfiguration.LoadFromEnvironment());

            Assert.Contains("LOGSEQ_PORT", exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", null);
            Environment.SetEnvironmentVariable("LOGSEQ_PORT", null);
        }
    }

    [Fact]
    public void Validate_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", "test-token-123");

        try
        {
            var config = LogseqConfiguration.LoadFromEnvironment();

            // Act & Assert
            var exception = Record.Exception(() => config.Validate());
            Assert.Null(exception);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOGSEQ_AUTH_TOKEN", null);
        }
    }
}
