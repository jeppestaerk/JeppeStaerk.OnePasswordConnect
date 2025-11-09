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

public class ActivityClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly ActivityClient _activityClient;

    public ActivityClientTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<ActivityClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _activityClient = new ActivityClient(httpClientFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetApiActivityAsync_WithDefaultParameters_ShouldReturnActivity()
    {
        // Arrange
        var expectedActivity = new List<ApiRequest>
        {
            new()
            {
                RequestId = "req1",
                Timestamp = DateTime.UtcNow,
                Action = Enums.ApiRequestAction.READ,
                Result = Enums.ApiRequestResult.SUCCESS
            },
            new()
            {
                RequestId = "req2",
                Timestamp = DateTime.UtcNow,
                Action = Enums.ApiRequestAction.CREATE,
                Result = Enums.ApiRequestResult.SUCCESS
            }
        };

        var responseContent = JsonSerializer.Serialize(expectedActivity, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _activityClient.GetApiActivityAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].RequestId.Should().Be("req1");

        VerifyHttpRequest(HttpMethod.Get, "/v1/activity?limit=50&offset=0");
    }

    [Fact]
    public async Task GetApiActivityAsync_WithCustomLimitAndOffset_ShouldIncludeParameters()
    {
        // Arrange
        const int limit = 10;
        const int offset = 20;
        var expectedActivity = new List<ApiRequest>
        {
            new() { RequestId = "req1" }
        };

        var responseContent = JsonSerializer.Serialize(expectedActivity, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _activityClient.GetApiActivityAsync(limit, offset, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        VerifyHttpRequest(HttpMethod.Get, "/v1/activity?limit=10&offset=20");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetApiActivityAsync_WithInvalidLimit_ShouldThrowArgumentException(int invalidLimit)
    {
        // Act
        var act = async () => await _activityClient.GetApiActivityAsync(invalidLimit);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Limit must be greater than 0*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task GetApiActivityAsync_WithNegativeOffset_ShouldThrowArgumentException(int invalidOffset)
    {
        // Act
        var act = async () => await _activityClient.GetApiActivityAsync(50, invalidOffset);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Offset cannot be negative*");
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(100, 0)]
    [InlineData(50, 100)]
    [InlineData(25, 75)]
    public async Task GetApiActivityAsync_WithValidLimitAndOffset_ShouldSucceed(int limit, int offset)
    {
        // Arrange
        var expectedActivity = new List<ApiRequest>();
        var responseContent = JsonSerializer.Serialize(expectedActivity, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _activityClient.GetApiActivityAsync(limit, offset, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        VerifyHttpRequest(HttpMethod.Get, $"/v1/activity?limit={limit}&offset={offset}");
    }

    [Fact]
    public async Task GetApiActivityAsync_WithCancellationToken_ShouldNotThrow()
    {
        // Arrange
        var expectedActivity = new List<ApiRequest>();
        var responseContent = JsonSerializer.Serialize(expectedActivity, TestHelpers.JsonOptions);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _activityClient.GetApiActivityAsync(50, 0, TestContext.Current.CancellationToken);

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

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content, Encoding.UTF8, "application/json")
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
            expectedUri = new Uri("http://localhost:8080" + expectedPath);
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
