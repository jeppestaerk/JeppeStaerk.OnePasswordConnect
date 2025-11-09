using FluentAssertions;
using JeppeStaerk.OnePasswordConnect.Sdk.Configuration;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

public class OnePasswordConnectOptionsTests
{
    [Fact]
    public void Constructor_Default_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var options = new OnePasswordConnectOptions();

        // Assert
        options.BaseUrl.Should().Be("http://localhost:8080");
        options.ApiToken.Should().BeNull();
        options.TimeoutSeconds.Should().Be(30);
        options.RetryCount.Should().Be(3);
        options.CircuitBreakerFailureThreshold.Should().Be(5);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(30);
    }

    [Fact]
    public void Constructor_WithApiToken_ShouldSetApiToken()
    {
        // Arrange
        const string apiToken = "test-token-123";

        // Act
        var options = new OnePasswordConnectOptions(apiToken);

        // Assert
        options.ApiToken.Should().Be(apiToken);
        options.BaseUrl.Should().Be("http://localhost:8080");
    }

    [Fact]
    public void Constructor_WithApiTokenAndBaseUrl_ShouldSetBothValues()
    {
        // Arrange
        const string apiToken = "test-token-123";
        const string baseUrl = "https://connect.example.com";

        // Act
        var options = new OnePasswordConnectOptions(apiToken, baseUrl);

        // Assert
        options.ApiToken.Should().Be(apiToken);
        options.BaseUrl.Should().Be("https://connect.example.com");
    }

    [Theory]
    [InlineData("http://localhost:8080")]
    [InlineData("http://localhost:8080/")]
    [InlineData("https://connect.example.com")]
    [InlineData("https://connect.example.com/")]
    public void BaseUrl_ShouldStoreAsProvided(string url)
    {
        // Arrange & Act
        var options = new OnePasswordConnectOptions { BaseUrl = url };

        // Assert - BaseUrl is stored as-is (no normalization)
        options.BaseUrl.Should().Be(url);
    }

    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token-123",
            BaseUrl = "https://connect.example.com"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithMissingApiToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            BaseUrl = "https://connect.example.com"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ApiToken is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidTimeoutSeconds_ShouldThrowInvalidOperationException(int invalidTimeout)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token-123",
            TimeoutSeconds = invalidTimeout
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TimeoutSeconds must be greater than 0*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNegativeRetryCount_ShouldThrowInvalidOperationException(int invalidRetryCount)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token-123",
            RetryCount = invalidRetryCount
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RetryCount cannot be negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidCircuitBreakerFailureThreshold_ShouldThrowInvalidOperationException(int invalidThreshold)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token-123",
            CircuitBreakerFailureThreshold = invalidThreshold
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CircuitBreakerFailureThreshold must be greater than 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidCircuitBreakerBreakDurationSeconds_ShouldThrowInvalidOperationException(int invalidDuration)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token-123",
            CircuitBreakerBreakDurationSeconds = invalidDuration
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CircuitBreakerBreakDurationSeconds must be greater than 0*");
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldIncludeAllErrors()
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = null, // Missing
            TimeoutSeconds = -1, // Invalid
            RetryCount = -5 // Invalid
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Configuration validation failed*")
            .Which.Message.Should().Contain("ApiToken");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(120)]
    [InlineData(int.MaxValue)]
    public void TimeoutSeconds_WithValidValues_ShouldAccept(int validTimeout)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token",
            TimeoutSeconds = validTimeout
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
        options.TimeoutSeconds.Should().Be(validTimeout);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(int.MaxValue)]
    public void RetryCount_WithValidValues_ShouldAccept(int validRetryCount)
    {
        // Arrange
        var options = new OnePasswordConnectOptions
        {
            ApiToken = "test-token",
            RetryCount = validRetryCount
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
        options.RetryCount.Should().Be(validRetryCount);
    }
}
