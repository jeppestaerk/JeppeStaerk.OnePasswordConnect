using FluentAssertions;
using JeppeStaerk.OnePasswordConnect.Sdk.Clients;
using JeppeStaerk.OnePasswordConnect.Sdk.Configuration;
using JeppeStaerk.OnePasswordConnect.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOnePasswordConnect_WithValidConfiguration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token-123";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<OnePasswordConnectClient>().Should().NotBeNull();
        serviceProvider.GetService<VaultsClient>().Should().NotBeNull();
        serviceProvider.GetService<ItemsClient>().Should().NotBeNull();
        serviceProvider.GetService<FilesClient>().Should().NotBeNull();
        serviceProvider.GetService<ActivityClient>().Should().NotBeNull();
        serviceProvider.GetService<HealthClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddOnePasswordConnect_WithSimplifiedOverload_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOnePasswordConnect(
            baseUrl: "https://connect.example.com",
            apiToken: "test-token-123");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<OnePasswordConnectClient>().Should().NotBeNull();
        serviceProvider.GetService<VaultsClient>().Should().NotBeNull();
        serviceProvider.GetService<ItemsClient>().Should().NotBeNull();
        serviceProvider.GetService<FilesClient>().Should().NotBeNull();
        serviceProvider.GetService<ActivityClient>().Should().NotBeNull();
        serviceProvider.GetService<HealthClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddOnePasswordConnect_ShouldRegisterServicesAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Get multiple instances
        var client1 = serviceProvider.GetService<OnePasswordConnectClient>();
        var client2 = serviceProvider.GetService<OnePasswordConnectClient>();

        // Assert - Should be different instances (transient)
        client1.Should().NotBeNull();
        client2.Should().NotBeNull();
        client1.Should().NotBeSameAs(client2);
    }

    [Fact]
    public void AddOnePasswordConnect_WithInvalidConfiguration_ShouldThrowOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = null; // Invalid - missing API token
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Validation happens when options are accessed
        var act = () => serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ApiToken*");
    }

    [Fact]
    public void AddOnePasswordConnect_WithInvalidTimeoutSeconds_ShouldThrowOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
            options.TimeoutSeconds = -1; // Invalid
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Validation happens when options are accessed
        var act = () => serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*TimeoutSeconds*");
    }

    [Fact]
    public void AddOnePasswordConnect_WithInvalidRetryCount_ShouldThrowOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
            options.RetryCount = -5; // Invalid
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Validation happens when options are accessed
        var act = () => serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*RetryCount*");
    }

    [Fact]
    public void AddOnePasswordConnect_WithInvalidCircuitBreakerThreshold_ShouldThrowOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
            options.CircuitBreakerFailureThreshold = 0; // Invalid
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Validation happens when options are accessed
        var act = () => serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*CircuitBreakerFailureThreshold*");
    }

    [Fact]
    public void AddOnePasswordConnect_ShouldRegisterHttpClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        // Assert
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddOnePasswordConnect_ShouldConfigureOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token-123";
            options.TimeoutSeconds = 60;
            options.RetryCount = 5;
            options.CircuitBreakerFailureThreshold = 10;
            options.CircuitBreakerBreakDurationSeconds = 45;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetService<IOptions<OnePasswordConnectOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.BaseUrl.Should().Be("https://connect.example.com");
        options.Value.ApiToken.Should().Be("test-token-123");
        options.Value.TimeoutSeconds.Should().Be(60);
        options.Value.RetryCount.Should().Be(5);
        options.Value.CircuitBreakerFailureThreshold.Should().Be(10);
        options.Value.CircuitBreakerBreakDurationSeconds.Should().Be(45);
    }

    [Fact]
    public void AddOnePasswordConnect_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => ServiceCollectionExtensions.AddOnePasswordConnect(
            null!,
            options => { options.ApiToken = "test"; });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddOnePasswordConnect_WithNullConfigureOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddOnePasswordConnect(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }

    [Fact]
    public void AddOnePasswordConnect_ShouldNormalizeBaseUrl()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetService<IOptions<OnePasswordConnectOptions>>();

        // Assert
        options!.Value.BaseUrl.Should().Be("https://connect.example.com");
    }

    [Fact]
    public void AddOnePasswordConnect_WithRetryCountZero_ShouldBeValid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
            options.RetryCount = 0; // Valid - disables retries
        });

        // Act
        var act = () => services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true
        });

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddOnePasswordConnect_ShouldAllowMultipleClients()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = "https://connect.example.com";
            options.ApiToken = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vaultsClient = serviceProvider.GetService<VaultsClient>();
        var itemsClient = serviceProvider.GetService<ItemsClient>();
        var filesClient = serviceProvider.GetService<FilesClient>();
        var activityClient = serviceProvider.GetService<ActivityClient>();
        var healthClient = serviceProvider.GetService<HealthClient>();

        // Assert
        vaultsClient.Should().NotBeNull();
        itemsClient.Should().NotBeNull();
        filesClient.Should().NotBeNull();
        activityClient.Should().NotBeNull();
        healthClient.Should().NotBeNull();
    }
}
