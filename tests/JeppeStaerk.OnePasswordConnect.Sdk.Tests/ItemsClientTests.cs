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

public class ItemsClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly ItemsClient _itemsClient;

    public ItemsClientTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<ItemsClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _itemsClient = new ItemsClient(httpClientFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetVaultItemsAsync_WithValidVaultId_ShouldReturnItems()
    {
        // Arrange
        const string vaultId = "test-vault-123";
        var expectedItems = new List<Item>
        {
            new() { Id = "item1", Title = "Test Item 1" },
            new() { Id = "item2", Title = "Test Item 2" }
        };

        var responseContent = JsonSerializer.Serialize(expectedItems, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.GetVaultItemsAsync(vaultId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("item1");

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items");
    }

    [Fact]
    public async Task GetVaultItemsAsync_WithFilter_ShouldEncodeFilterParameter()
    {
        // Arrange
        const string vaultId = "vault123";
        const string filter = "title eq \"Database Password\"";
        var expectedItems = new List<Item>
        {
            new() { Id = "item1", Title = "Database Password" }
        };

        var responseContent = JsonSerializer.Serialize(expectedItems, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.GetVaultItemsAsync(vaultId, filter, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault123/items?filter=title%20eq%20%22Database%20Password%22");
    }

    [Fact]
    public async Task GetVaultItemsAsync_WithSpecialCharactersInVaultId_ShouldEncodeVaultId()
    {
        // Arrange
        const string vaultId = "vault with spaces";
        var expectedItems = new List<Item> { new() { Id = "item1" } };

        var responseContent = JsonSerializer.Serialize(expectedItems, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.GetVaultItemsAsync(vaultId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();

        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault%20with%20spaces/items");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetVaultItemsAsync_WithInvalidVaultId_ShouldThrowArgumentException(string? invalidVaultId)
    {
        // Act
        var act = async () => await _itemsClient.GetVaultItemsAsync(invalidVaultId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Vault ID cannot be null or whitespace*");
    }

    [Fact]
    public async Task GetVaultItemByIdAsync_WithValidIds_ShouldReturnItem()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        var expectedItem = new FullItem
        {
            Id = itemId,
            Title = "Test Item",
            Vault = new VaultReference { Id = vaultId }
        };

        var responseContent = JsonSerializer.Serialize(expectedItem, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.GetVaultItemByIdAsync(vaultId, itemId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(itemId);
        result.Title.Should().Be("Test Item");

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}");
    }

    [Fact]
    public async Task GetVaultItemByIdAsync_WithSpecialCharactersInIds_ShouldEncodeBothIds()
    {
        // Arrange
        const string vaultId = "vault/with/slashes";
        const string itemId = "item with spaces";
        var expectedItem = new FullItem
        {
            Id = itemId,
            Title = "Test Item"
        };

        var responseContent = JsonSerializer.Serialize(expectedItem, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.GetVaultItemByIdAsync(vaultId, itemId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();

        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault%2Fwith%2Fslashes/items/item%20with%20spaces");
    }

    [Theory]
    [InlineData(null, "item123")]
    [InlineData("", "item123")]
    [InlineData("   ", "item123")]
    [InlineData("vault123", null)]
    [InlineData("vault123", "")]
    [InlineData("vault123", "   ")]
    public async Task GetVaultItemByIdAsync_WithInvalidIds_ShouldThrowArgumentException(string? vaultId, string? itemId)
    {
        // Act
        var act = async () => await _itemsClient.GetVaultItemByIdAsync(vaultId!, itemId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateVaultItemAsync_WithValidItem_ShouldReturnCreatedItem()
    {
        // Arrange
        const string vaultId = "vault123";
        var newItem = new FullItem
        {
            Title = "New Item",
            Category = Enums.ItemCategory.LOGIN
        };
        var createdItem = new FullItem
        {
            Id = "newitem123",
            Title = "New Item",
            Category = Enums.ItemCategory.LOGIN
        };

        var responseContent = JsonSerializer.Serialize(createdItem, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.Created, responseContent);

        // Act
        var result = await _itemsClient.CreateVaultItemAsync(vaultId, newItem, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("newitem123");

        VerifyHttpRequest(HttpMethod.Post, $"/v1/vaults/{vaultId}/items");
    }

    [Fact]
    public async Task CreateVaultItemAsync_WithNullItem_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _itemsClient.CreateVaultItemAsync("vault123", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("item");
    }

    [Fact]
    public async Task UpdateVaultItemAsync_WithValidData_ShouldReturnUpdatedItem()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        var updatedItem = new FullItem
        {
            Id = itemId,
            Title = "Updated Title"
        };

        var responseContent = JsonSerializer.Serialize(updatedItem, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.UpdateVaultItemAsync(vaultId, itemId, updatedItem, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");

        VerifyHttpRequest(HttpMethod.Put, $"/v1/vaults/{vaultId}/items/{itemId}");
    }

    [Fact]
    public async Task DeleteVaultItemAsync_WithValidIds_ShouldSucceed()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";

        SetupHttpResponse(HttpStatusCode.NoContent, string.Empty);

        // Act
        var act = async () => await _itemsClient.DeleteVaultItemAsync(vaultId, itemId);

        // Assert
        await act.Should().NotThrowAsync();

        VerifyHttpRequest(HttpMethod.Delete, $"/v1/vaults/{vaultId}/items/{itemId}");
    }

    [Fact]
    public async Task PatchVaultItemAsync_WithValidPatchOperations_ShouldReturnUpdatedItem()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        var patchOperations = new List<PatchOperation>
        {
            new()
            {
                Op = Enums.PatchOperation.replace,
                Path = "/title",
                Value = "Patched Title"
            }
        };

        var patchedItem = new FullItem
        {
            Id = itemId,
            Title = "Patched Title"
        };

        var responseContent = JsonSerializer.Serialize(patchedItem, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _itemsClient.PatchVaultItemAsync(vaultId, itemId, patchOperations, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Patched Title");

        // Note: Verifying PATCH requires checking for HttpMethod.Patch which may not be available in all frameworks
        VerifyHttpRequest(HttpMethod.Patch, $"/v1/vaults/{vaultId}/items/{itemId}");
    }

    [Theory]
    [InlineData(null)]
    public async Task PatchVaultItemAsync_WithNullPatchOperations_ShouldThrowArgumentException(List<PatchOperation>? patchOps)
    {
        // Act
        var act = async () => await _itemsClient.PatchVaultItemAsync("vault123", "item456", patchOps!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Patch operations cannot be null or empty*");
    }

    [Fact]
    public async Task PatchVaultItemAsync_WithEmptyPatchOperations_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _itemsClient.PatchVaultItemAsync("vault123", "item456", []);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Patch operations cannot be null or empty*");
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
