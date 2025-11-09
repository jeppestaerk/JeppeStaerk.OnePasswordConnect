using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Configuration;

/// <summary>
/// Configuration options for the 1Password Connect API client.
/// </summary>
public class OnePasswordConnectOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectOptions"/> class.
    /// </summary>
    public OnePasswordConnectOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectOptions"/> class with an API token.
    /// </summary>
    /// <param name="apiToken">The API token (JWT) used for authentication.</param>
    public OnePasswordConnectOptions(string apiToken)
    {
        ApiToken = apiToken;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectOptions"/> class with an API token and base URL.
    /// </summary>
    /// <param name="apiToken">The API token (JWT) used for authentication.</param>
    /// <param name="baseUrl">The base URL of the 1Password Connect API server. /v1/ will be appended if not present.</param>
    public OnePasswordConnectOptions(string apiToken, string baseUrl)
    {
        ApiToken = apiToken;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// The base URL of the 1Password Connect API server.
    /// Should be the root URL without /v1 (e.g., "http://localhost:8080").
    /// Client paths include the /v1 prefix automatically.
    /// Default: http://localhost:8080
    /// </summary>
    [Required(ErrorMessage = "BaseUrl is required.")]
    [Url(ErrorMessage = "BaseUrl must be a valid URL.")]
    public string BaseUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// The API token (JWT) used for authentication.
    /// </summary>
    [Required(ErrorMessage = "ApiToken is required for 1Password Connect API authentication.")]
    public string? ApiToken { get; set; }

    /// <summary>
    /// Optional timeout for HTTP requests in seconds.
    /// Default: 30 seconds
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "TimeoutSeconds must be greater than 0.")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient HTTP errors (network failures, 5xx errors, 408 timeout).
    /// Uses exponential backoff: 2s, 4s, 8s between retries.
    /// Default: 3
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "RetryCount cannot be negative.")]
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Number of consecutive failures before the circuit breaker opens.
    /// When open, requests fail immediately without attempting to reach the server.
    /// Default: 5
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "CircuitBreakerFailureThreshold must be greater than 0.")]
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duration in seconds that the circuit breaker stays open before attempting to recover.
    /// During this time, all requests fail immediately.
    /// Default: 30 seconds
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "CircuitBreakerBreakDurationSeconds must be greater than 0.")]
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the configuration options using data annotations.
    /// This method is useful when creating options manually (not via dependency injection).
    /// When using dependency injection, validation happens automatically via ValidateDataAnnotations().
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Configuration validation failed: {errors}");
        }
    }
}
