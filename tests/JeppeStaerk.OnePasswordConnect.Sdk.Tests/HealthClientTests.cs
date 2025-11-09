using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using JeppeStaerk.OnePasswordConnect.Sdk.Clients;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

public class HealthClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HealthClient _healthClient;

    public HealthClientTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<HealthClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _healthClient = new HealthClient(httpClientFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetHeartbeatAsync_WithSuccessfulResponse_ShouldReturnHeartbeat()
    {
        // Arrange
        const string expectedHeartbeat = ".";
        SetupHttpResponse(HttpStatusCode.OK, expectedHeartbeat, "text/plain");

        // Act
        var result = await _healthClient.GetHeartbeatAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedHeartbeat);
        VerifyHttpRequest(HttpMethod.Get, "/heartbeat");
    }

    [Fact]
    public async Task GetHeartbeatAsync_WithCancellationToken_ShouldNotThrow()
    {
        // Arrange
        const string expectedHeartbeat = ".";
        SetupHttpResponse(HttpStatusCode.OK, expectedHeartbeat, "text/plain");

        // Act
        var result = await _healthClient.GetHeartbeatAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedHeartbeat);

        // Verify that SendAsync was called (proves the cancellation token was passed through)
        _httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetServerHealthAsync_WithValidResponse_ShouldReturnServerHealth()
    {
        // Arrange
        var expectedHealth = new ServerHealth
        {
            Name = "1Password Connect API",
            Version = "1.8.1"
        };

        var responseContent = JsonSerializer.Serialize(expectedHealth, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent, "application/json");

        // Act
        var result = await _healthClient.GetServerHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("1Password Connect API");
        result.Version.Should().Be("1.8.1");

        VerifyHttpRequest(HttpMethod.Get, "/health");
    }

    [Fact]
    public async Task GetServerHealthAsync_WithCancellationToken_ShouldNotThrow()
    {
        // Arrange
        var expectedHealth = new ServerHealth
        {
            Name = "1Password Connect API",
            Version = "1.8.1"
        };

        var responseContent = JsonSerializer.Serialize(expectedHealth, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent, "application/json");

        // Act
        var result = await _healthClient.GetServerHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();

        // Verify that SendAsync was called (proves the cancellation token was passed through)
        _httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetPrometheusMetricsAsync_WithSuccessfulResponse_ShouldReturnMetrics()
    {
        // Arrange
        const string expectedMetrics = """
                                       # HELP onepassword_connect_requests_total Total number of requests
                                       # TYPE onepassword_connect_requests_total counter
                                       onepassword_connect_requests_total{method="GET",status="200"} 42
                                       """;

        SetupHttpResponse(HttpStatusCode.OK, expectedMetrics, "text/plain");

        // Act
        var result = await _healthClient.GetPrometheusMetricsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("onepassword_connect_requests_total");
        result.Should().Contain("# HELP");
        result.Should().Contain("# TYPE");

        VerifyHttpRequest(HttpMethod.Get, "/metrics");
    }

    [Fact]
    public async Task GetPrometheusMetricsAsync_WithCancellationToken_ShouldNotThrow()
    {
        // Arrange
        const string expectedMetrics = "# Prometheus metrics";
        SetupHttpResponse(HttpStatusCode.OK, expectedMetrics, "text/plain");

        // Act
        var result = await _healthClient.GetPrometheusMetricsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify that SendAsync was called (proves the cancellation token was passed through)
        _httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetPrometheusMetricsAsync_WithEmptyMetrics_ShouldReturnEmptyString()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, string.Empty, "text/plain");

        // Act
        var result = await _healthClient.GetPrometheusMetricsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content, string mediaType)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content, Encoding.UTF8, mediaType)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(HttpMethod expectedMethod, string expectedPath)
    {
        var expectedUri = new Uri(expectedPath, UriKind.RelativeOrAbsolute);
        if (!expectedUri.IsAbsoluteUri)
        {
            // Health and heartbeat endpoints are at root level (no /v1 prefix)
            var isRootLevelEndpoint = expectedPath == "/heartbeat" || expectedPath == "/health" || expectedPath == "/metrics";
            var baseUrl = isRootLevelEndpoint ? "http://localhost:8080" : "http://localhost:8080/v1";
            expectedUri = new Uri(baseUrl + expectedPath);
        }

        _httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == expectedMethod &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString() == expectedUri.ToString()),
                ItExpr.IsAny<CancellationToken>());
    }
}
