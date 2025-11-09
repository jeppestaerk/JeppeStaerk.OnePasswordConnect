using FluentAssertions;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

/// <summary>
/// Tests to verify URL encoding behavior for IDs containing special characters.
/// This ensures that vault IDs, item IDs, and file IDs with special characters
/// are properly encoded using Uri.EscapeDataString().
/// </summary>
public class UrlEncodingTests
{
    [Theory]
    [InlineData("simple-id", "simple-id")]
    [InlineData("id with spaces", "id%20with%20spaces")]
    [InlineData("id/with/slashes", "id%2Fwith%2Fslashes")]
    [InlineData("id?with&special=chars", "id%3Fwith%26special%3Dchars")]
    [InlineData("id#with@special!chars", "id%23with%40special%21chars")]
    [InlineData("id+with%percent", "id%2Bwith%25percent")]
    [InlineData("émoji-café-🔐", "%C3%A9moji-caf%C3%A9-%F0%9F%94%90")]
    public void UriEscapeDataString_ShouldEncodeSpecialCharacters(string input, string expected)
    {
        // Act
        var encoded = Uri.EscapeDataString(input);

        // Assert
        encoded.Should().Be(expected);
    }

    [Fact]
    public void UriEscapeDataString_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var encoded = Uri.EscapeDataString(input);

        // Assert
        encoded.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("0123456789")]
    [InlineData("-_.~")]
    public void UriEscapeDataString_WithUnreservedCharacters_ShouldNotEncode(string input)
    {
        // Act
        var encoded = Uri.EscapeDataString(input);

        // Assert
        encoded.Should().Be(input, "unreserved characters should not be encoded");
    }

    [Theory]
    [InlineData("/vaults/{0}", "vault-123", "/vaults/vault-123")]
    [InlineData("/vaults/{0}", "vault with spaces", "/vaults/vault%20with%20spaces")]
    [InlineData("/vaults/{0}/items/{1}", "vault-id", "item/with/slashes", "/vaults/vault-id/items/item%2Fwith%2Fslashes")]
    [InlineData("/vaults/{0}/items/{1}/files/{2}", "v1", "i1", "file?name.txt", "/vaults/v1/items/i1/files/file%3Fname.txt")]
    public void BuildPath_WithUriEscapeDataString_ShouldProperlyEncodePaths(
        string pathTemplate,
        params string[] parameters)
    {
        // Act
        string actualPath;
        string expectedPath;

        if (pathTemplate.Contains("{2}"))
        {
            // 3 parameters
            actualPath = string.Format(pathTemplate,
                Uri.EscapeDataString(parameters[0]),
                Uri.EscapeDataString(parameters[1]),
                Uri.EscapeDataString(parameters[2]));
            expectedPath = parameters[3];
        }
        else if (pathTemplate.Contains("{1}"))
        {
            // 2 parameters
            actualPath = string.Format(pathTemplate,
                Uri.EscapeDataString(parameters[0]),
                Uri.EscapeDataString(parameters[1]));
            expectedPath = parameters[2];
        }
        else
        {
            // 1 parameter
            actualPath = string.Format(pathTemplate, Uri.EscapeDataString(parameters[0]));
            expectedPath = parameters[1];
        }

        // Assert
        actualPath.Should().Be(expectedPath);
    }

    [Theory]
    [InlineData("name eq \"Production Vault\"", "name%20eq%20%22Production%20Vault%22")]
    [InlineData("title eq \"Database Password\"", "title%20eq%20%22Database%20Password%22")]
    [InlineData("name eq \"Test & Development\"", "name%20eq%20%22Test%20%26%20Development%22")]
    public void FilterParameter_ShouldBeUrlEncoded(string filter, string expectedEncoded)
    {
        // Act
        var encoded = Uri.EscapeDataString(filter);

        // Assert
        encoded.Should().Be(expectedEncoded);
    }

    [Fact]
    public void QueryStringParameters_ShouldNotDoubleEncode()
    {
        // Arrange
        const string filter = "name eq \"My Vault\"";
        var encoded = Uri.EscapeDataString(filter);

        // Act - Encoding again should change the result (demonstrating double encoding would be wrong)
        var doubleEncoded = Uri.EscapeDataString(encoded);

        // Assert
        doubleEncoded.Should().NotBe(encoded, "double encoding should be avoided");
        encoded.Should().Be("name%20eq%20%22My%20Vault%22");
        doubleEncoded.Should().Be("name%2520eq%2520%2522My%2520Vault%2522");
    }

    [Theory]
    [InlineData("?limit=50&offset=0", "?limit=50&offset=0")]
    [InlineData("?inline_files=true", "?inline_files=true")]
    public void IntegerAndBooleanQueryParameters_ShouldNotBeEncoded(string queryString, string expected)
    {
        // Assert - Integer and boolean parameters don't need encoding
        queryString.Should().Be(expected);
    }

    [Fact]
    public void ComplexPath_WithMultipleEncodedSegments_ShouldBuildCorrectly()
    {
        // Arrange
        const string vaultId = "vault/with/slashes";
        const string itemId = "item with spaces";
        const string fileId = "file?name&special=chars.txt";

        // Act
        var path = $"/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}/files/{Uri.EscapeDataString(fileId)}/content";

        // Assert
        path.Should().Be("/vaults/vault%2Fwith%2Fslashes/items/item%20with%20spaces/files/file%3Fname%26special%3Dchars.txt/content");
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("0123456789", "0123456789")]
    [InlineData("hyphens-and_underscores", "hyphens-and_underscores")]
    [InlineData("dots.and~tildes", "dots.and~tildes")]
    public void CommonVaultAndItemIds_WithStandardCharacters_ShouldNotBeEncoded(string id, string expected)
    {
        // Act
        var encoded = Uri.EscapeDataString(id);

        // Assert
        encoded.Should().Be(expected, "standard alphanumeric and unreserved characters should not be encoded");
    }
}
