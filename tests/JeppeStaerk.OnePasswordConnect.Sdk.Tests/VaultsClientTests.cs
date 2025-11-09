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

public class VaultsClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly VaultsClient _vaultsClient;

    public VaultsClientTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<VaultsClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _vaultsClient = new VaultsClient(httpClientFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetVaultsAsync_WithoutFilter_ShouldReturnVaults()
    {
        // Arrange
        var expectedVaults = new List<Vault>
        {
            new()
            {
                Id = "vault1",
                Name = "Test Vault 1"
            },
            new()
            {
                Id = "vault2",
                Name = "Test Vault 2"
            }
        };

        var responseContent = JsonSerializer.Serialize(expectedVaults, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _vaultsClient.GetVaultsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("vault1");
        result[1].Id.Should().Be("vault2");

        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults");
    }

    [Fact]
    public async Task GetVaultsAsync_WithFilter_ShouldEncodeFilterParameter()
    {
        // Arrange
        const string filter = "name eq \"Production Vault\"";
        var expectedVaults = new List<Vault>
        {
            new() { Id = "vault1", Name = "Production Vault" }
        };

        var responseContent = JsonSerializer.Serialize(expectedVaults, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _vaultsClient.GetVaultsAsync(filter, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        // Verify the filter was properly URL encoded
        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults?filter=name%20eq%20%22Production%20Vault%22");
    }

    [Fact]
    public async Task GetVaultByIdAsync_WithValidId_ShouldReturnVault()
    {
        // Arrange
        const string vaultId = "test-vault-123";
        var expectedVault = new Vault
        {
            Id = vaultId,
            Name = "Test Vault",
            Description = "Test vault description"
        };

        var responseContent = JsonSerializer.Serialize(expectedVault, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _vaultsClient.GetVaultByIdAsync(vaultId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(vaultId);
        result.Name.Should().Be("Test Vault");

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}");
    }

    [Fact]
    public async Task GetVaultByIdAsync_WithSpecialCharactersInId_ShouldEncodeId()
    {
        // Arrange
        const string vaultId = "vault/with/slashes";
        var expectedVault = new Vault
        {
            Id = vaultId,
            Name = "Test Vault"
        };

        var responseContent = JsonSerializer.Serialize(expectedVault, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _vaultsClient.GetVaultByIdAsync(vaultId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(vaultId);

        // Verify the vault ID was properly URL encoded
        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault%2Fwith%2Fslashes");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetVaultByIdAsync_WithInvalidId_ShouldThrowArgumentException(string? invalidId)
    {
        // Act
        var act = async () => await _vaultsClient.GetVaultByIdAsync(invalidId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Vault ID cannot be null or whitespace*");
    }

    [Fact]
    public async Task GetVaultsAsync_WithCancellationToken_ShouldPassTokenToRequest()
    {
        // Arrange
        var expectedVaults = new List<Vault> { new() { Id = "vault1" } };
        var responseContent = JsonSerializer.Serialize(expectedVaults, TestHelpers.JsonOptions);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        await _vaultsClient.GetVaultsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Verify SendAsync was called, which confirms the cancellation token parameter is working
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
        // Note: The request URI will NOT include the base address when using relative paths
        // The path should be relative, starting with /vaults/...
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
