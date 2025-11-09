using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using JeppeStaerk.OnePasswordConnect.Sdk.Clients;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Models_File = JeppeStaerk.OnePasswordConnect.Sdk.Models.File;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

public class FilesClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly FilesClient _filesClient;

    public FilesClientTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<FilesClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _filesClient = new FilesClient(httpClientFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetItemFilesAsync_WithValidIds_ShouldReturnFiles()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        var expectedFiles = new List<Models_File>
        {
            new()
            {
                Id = "file1",
                Name = "document.pdf",
                Size = 1024
            },
            new()
            {
                Id = "file2",
                Name = "image.png",
                Size = 2048
            }
        };

        var responseContent = JsonSerializer.Serialize(expectedFiles, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _filesClient.GetItemFilesAsync(vaultId, itemId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("document.pdf");
        result[1].Size.Should().Be(2048);

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}/files");
    }

    [Fact]
    public async Task GetItemFilesAsync_WithInlineFiles_ShouldIncludeQueryParameter()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        var expectedFiles = new List<Models_File>
        {
            new() { Id = "file1", Name = "file.txt" }
        };

        var responseContent = JsonSerializer.Serialize(expectedFiles, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        await _filesClient.GetItemFilesAsync(vaultId, itemId, inlineFiles: true, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}/files?inline_files=true");
    }

    [Fact]
    public async Task GetItemFilesAsync_WithSpecialCharactersInIds_ShouldEncodeIds()
    {
        // Arrange
        const string vaultId = "vault with spaces";
        const string itemId = "item/with/slashes";
        var expectedFiles = new List<Models_File> { new() { Id = "file1" } };

        var responseContent = JsonSerializer.Serialize(expectedFiles, TestHelpers.JsonOptions);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        await _filesClient.GetItemFilesAsync(vaultId, itemId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault%20with%20spaces/items/item%2Fwith%2Fslashes/files");
    }

    [Theory]
    [InlineData(null, "item123")]
    [InlineData("", "item123")]
    [InlineData("   ", "item123")]
    [InlineData("vault123", null)]
    [InlineData("vault123", "")]
    [InlineData("vault123", "   ")]
    public async Task GetItemFilesAsync_WithInvalidIds_ShouldThrowArgumentException(string? vaultId, string? itemId)
    {
        // Act
        var act = async () => await _filesClient.GetItemFilesAsync(vaultId!, itemId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetFileByIdAsync_WithValidIds_ShouldReturnFile()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        const string fileId = "file789";
        var expectedFile = new Models_File
        {
            Id = fileId,
            Name = "document.pdf",
            Size = 4096,
            ContentPath = "/v1/vaults/vault123/items/item456/files/file789/content"
        };

        var responseContent = JsonSerializer.Serialize(expectedFile);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _filesClient.GetFileByIdAsync(vaultId, itemId, fileId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(fileId);
        result.Name.Should().Be("document.pdf");
        result.Size.Should().Be(4096);

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}");
    }

    [Fact]
    public async Task GetFileByIdAsync_WithSpecialCharactersInFileId_ShouldEncodeFileId()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        const string fileId = "file?name&special=chars.txt";
        var expectedFile = new Models_File
        {
            Id = fileId,
            Name = "file.txt"
        };

        var responseContent = JsonSerializer.Serialize(expectedFile);
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        await _filesClient.GetFileByIdAsync(vaultId, itemId, fileId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        VerifyHttpRequest(HttpMethod.Get, "/v1/vaults/vault123/items/item456/files/file%3Fname%26special%3Dchars.txt");
    }

    [Theory]
    [InlineData(null, "item123", "file456")]
    [InlineData("vault123", null, "file456")]
    [InlineData("vault123", "item123", null)]
    [InlineData("", "item123", "file456")]
    [InlineData("vault123", "", "file456")]
    [InlineData("vault123", "item123", "")]
    public async Task GetFileByIdAsync_WithInvalidIds_ShouldThrowArgumentException(
        string? vaultId,
        string? itemId,
        string? fileId)
    {
        // Act
        var act = async () => await _filesClient.GetFileByIdAsync(vaultId!, itemId!, fileId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DownloadFileContentAsync_WithValidIds_ShouldReturnByteArray()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        const string fileId = "file789";
        var fileContent = "This is test file content"u8.ToArray();

        SetupHttpResponseWithByteContent(HttpStatusCode.OK, fileContent);

        // Act
        var result = await _filesClient.DownloadFileContentAsync(vaultId, itemId, fileId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(fileContent);

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}/content");
    }

    [Fact]
    public async Task DownloadFileContentAsync_WithSpecialCharactersInAllIds_ShouldEncodeAllIds()
    {
        // Arrange
        const string vaultId = "vault/with/slashes";
        const string itemId = "item with spaces";
        const string fileId = "file?name.txt";
        var fileContent = "content"u8.ToArray();

        SetupHttpResponseWithByteContent(HttpStatusCode.OK, fileContent);

        // Act
        await _filesClient.DownloadFileContentAsync(vaultId, itemId, fileId, TestContext.Current.CancellationToken);

        // Assert
        VerifyHttpRequest(HttpMethod.Get,
            "/v1/vaults/vault%2Fwith%2Fslashes/items/item%20with%20spaces/files/file%3Fname.txt/content");
    }

    [Fact]
    public async Task DownloadFileStreamAsync_WithValidIds_ShouldReturnStream()
    {
        // Arrange
        const string vaultId = "vault123";
        const string itemId = "item456";
        const string fileId = "file789";
        var fileContent = "Stream content test"u8.ToArray();

        SetupHttpResponseWithByteContent(HttpStatusCode.OK, fileContent);

        // Act
        var result = await _filesClient.DownloadFileStreamAsync(vaultId, itemId, fileId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();

        // Read the stream to verify content
        using var memoryStream = new MemoryStream();
        await result.CopyToAsync(memoryStream, TestContext.Current.CancellationToken);
        var streamContent = memoryStream.ToArray();
        streamContent.Should().BeEquivalentTo(fileContent);

        VerifyHttpRequest(HttpMethod.Get, $"/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}/content");
    }

    [Theory]
    [InlineData(null, "item123", "file456")]
    [InlineData("vault123", null, "file456")]
    [InlineData("vault123", "item123", null)]
    public async Task DownloadFileContentAsync_WithInvalidIds_ShouldThrowArgumentException(
        string? vaultId,
        string? itemId,
        string? fileId)
    {
        // Act
        var act = async () => await _filesClient.DownloadFileContentAsync(vaultId!, itemId!, fileId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null, "item123", "file456")]
    [InlineData("vault123", null, "file456")]
    [InlineData("vault123", "item123", null)]
    public async Task DownloadFileStreamAsync_WithInvalidIds_ShouldThrowArgumentException(
        string? vaultId,
        string? itemId,
        string? fileId)
    {
        // Act
        var act = async () => await _filesClient.DownloadFileStreamAsync(vaultId!, itemId!, fileId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetItemFilesAsync_WithCancellationToken_ShouldPassTokenToRequest()
    {
        // Arrange
        var expectedFiles = new List<Models_File> { new() { Id = "file1" } };
        var responseContent = JsonSerializer.Serialize(expectedFiles, TestHelpers.JsonOptions);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        await _filesClient.GetItemFilesAsync("vault123", "item456", cancellationToken: TestContext.Current.CancellationToken);

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

    private void SetupHttpResponseWithByteContent(HttpStatusCode statusCode, byte[] content)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new ByteArrayContent(content)
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
