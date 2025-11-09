using System;
using System.Net.Http;
using System.Net.Http.Headers;
using JeppeStaerk.OnePasswordConnect.Sdk.Clients;
using JeppeStaerk.OnePasswordConnect.Sdk.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Extensions;

/// <summary>
/// Extension methods for configuring 1Password Connect services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds 1Password Connect API client to the service collection.
    /// Uses IHttpClientFactory for optimal HttpClient lifecycle management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOnePasswordConnect(
        this IServiceCollection services,
        Action<OnePasswordConnectOptions> configureOptions)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
#endif
        
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configureOptions);
#else
        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }
#endif

        // Configure options with validation
        services.AddOptions<OnePasswordConnectOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register named HttpClient with Polly resilience policies
        var httpClientBuilder = services.AddHttpClient(typeof(BaseClient).FullName!, (serviceProvider, httpClient) =>
        {
            var opts = serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;
            // Note: Validation happens automatically via ValidateOnStart() during app startup
            httpClient.BaseAddress = new Uri(opts.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", opts.ApiToken);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Add retry policy with exponential backoff using policy handler factory
        httpClientBuilder.AddPolicyHandler((serviceProvider, _) =>
        {
            var opts = serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<OnePasswordConnectClient>>();

            // Only create retry policy if RetryCount > 0
            if (opts.RetryCount <= 0)
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: opts.RetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, _) =>
                    {
                        logger.LogWarning(
                            "Retry {RetryAttempt} after {Delay}s due to: {Exception}",
                            retryAttempt,
                            timespan.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown error");
                    });
        });

        // Add circuit breaker policy using policy handler factory
        httpClientBuilder.AddPolicyHandler((serviceProvider, _) =>
        {
            var opts = serviceProvider.GetRequiredService<IOptions<OnePasswordConnectOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<OnePasswordConnectClient>>();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: opts.CircuitBreakerFailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(opts.CircuitBreakerBreakDurationSeconds),
                    onBreak: (outcome, duration) =>
                    {
                        logger.LogError(
                            "Circuit breaker opened for {Duration}s due to: {Exception}",
                            duration.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown error");
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("Circuit breaker reset - connection restored");
                    });
        });

        // Register specialized clients
        services.AddTransient<VaultsClient>();
        services.AddTransient<ItemsClient>();
        services.AddTransient<FilesClient>();
        services.AddTransient<ActivityClient>();
        services.AddTransient<HealthClient>();

        // Register the main client
        services.AddTransient<OnePasswordConnectClient>();

        return services;
    }

    /// <summary>
    /// Adds 1Password Connect API client to the service collection using direct configuration.
    /// Uses IHttpClientFactory for optimal HttpClient lifecycle management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="baseUrl">The base URL of the 1Password Connect API.</param>
    /// <param name="apiToken">The API token for authentication.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOnePasswordConnect(
        this IServiceCollection services,
        string baseUrl,
        string apiToken)
    {
        return services.AddOnePasswordConnect(options =>
        {
            options.BaseUrl = baseUrl;
            options.ApiToken = apiToken;
        });
    }
}
