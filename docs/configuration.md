# Configuration

Complete guide to configuring the JeppeStaerk.OnePasswordConnect SDK.

## Table of Contents

- [Configuration Options](#configuration-options)
- [BaseUrl Configuration](#baseurl-configuration)
- [Advanced Configuration](#advanced-configuration)
- [Configuration Validation](#configuration-validation)
- [Environment-Specific Configuration](#environment-specific-configuration)

## Configuration Options

The `OnePasswordConnectOptions` class provides comprehensive configuration for the 1Password Connect client.

### All Available Options

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    // ===== Required Settings =====

    // Base URL of the 1Password Connect server (default: http://localhost:8080)
    // Should be just the base URL - client paths include /v1/ automatically
    options.BaseUrl = "https://connect.example.com";

    // API token for authentication (required)
    options.ApiToken = "your-token-here";


    // ===== Optional Settings =====

    // Request timeout in seconds (default: 30)
    options.TimeoutSeconds = 60;

    // Retry policy settings
    options.RetryCount = 3; // Number of retries for transient errors (default: 3)
                           // Set to 0 to disable retries

    // Circuit breaker settings
    options.CircuitBreakerFailureThreshold = 5;      // Failures before circuit opens (default: 5)
    options.CircuitBreakerBreakDurationSeconds = 30; // Seconds circuit stays open (default: 30)
});
```

### Property Reference

| Property | Type | Default | Description | Validation |
|----------|------|---------|-------------|------------|
| `BaseUrl` | string | `"http://localhost:8080"` | Root URL of 1Password Connect server (without `/v1`) | Required, must be valid URL |
| `ApiToken` | string | null | JWT token for authentication | Required, cannot be null/empty |
| `TimeoutSeconds` | int | 30 | HTTP request timeout in seconds | Must be > 0 |
| `RetryCount` | int | 3 | Number of retry attempts for transient errors | Must be >= 0 (0 disables retries) |
| `CircuitBreakerFailureThreshold` | int | 5 | Consecutive failures before circuit opens | Must be > 0 |
| `CircuitBreakerBreakDurationSeconds` | int | 30 | Duration circuit stays open | Must be > 0 |

## BaseUrl Configuration

The `BaseUrl` should be set to the root URL of your 1Password Connect server **without** the `/v1` suffix.

### Correct BaseUrl Formats

```csharp
// ✅ Correct - base URL only
options.BaseUrl = "https://connect.example.com";
options.BaseUrl = "http://localhost:8080";

// ✅ Also works - trailing slash is fine
options.BaseUrl = "https://connect.example.com/";

// ❌ Wrong - do not include /v1
options.BaseUrl = "https://connect.example.com/v1";
```

### How Client Paths Work

The SDK client paths automatically include the `/v1/` prefix for API endpoints:

- **Standard endpoints:** `/v1/vaults`, `/v1/items`, `/v1/activity`, `/v1/files`
- **Health endpoints:** `/heartbeat`, `/health`, `/metrics` (at root level, no `/v1/`)

### Example URL Construction

```
BaseUrl:     "http://localhost:8080"
Client path: "/v1/vaults"
Result:      "http://localhost:8080/v1/vaults"

BaseUrl:     "http://localhost:8080"
Client path: "/heartbeat"
Result:      "http://localhost:8080/heartbeat"
```

## Advanced Configuration

### Disable Retries

For scenarios where you want immediate failures without retries:

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    options.RetryCount = 0; // No retries
});
```

### Aggressive Circuit Breaker

For scenarios where you want to fail fast:

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    options.CircuitBreakerFailureThreshold = 3;  // Open after 3 failures
    options.CircuitBreakerBreakDurationSeconds = 60; // Stay open for 1 minute
});
```

### Lenient Settings for Unreliable Networks

For development or unreliable network conditions:

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    options.TimeoutSeconds = 60;                      // Longer timeout
    options.RetryCount = 5;                           // More retries
    options.CircuitBreakerFailureThreshold = 10;      // More tolerant
    options.CircuitBreakerBreakDurationSeconds = 15;  // Shorter break
});
```

## Configuration Validation

Configuration is **automatically validated at application startup** using data annotations. Invalid configuration will prevent the application from starting with clear error messages.

### Validation Example

```csharp
// This will fail at startup with a clear validation error
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "not-a-valid-url";  // ❌ Error: BaseUrl must be a valid URL
    options.TimeoutSeconds = -1;          // ❌ Error: TimeoutSeconds must be greater than 0
});
```

### Validation Errors

| Error Message | Cause | Fix |
|---------------|-------|-----|
| `BaseUrl is required` | BaseUrl is null or empty | Provide a valid URL |
| `BaseUrl must be a valid URL` | BaseUrl is not properly formatted | Use format: `http://localhost:8080` |
| `ApiToken is required` | ApiToken is null or empty | Provide your API token |
| `TimeoutSeconds must be greater than 0` | Timeout is 0 or negative | Use a positive value (e.g., 30) |
| `RetryCount cannot be negative` | RetryCount is negative | Use 0 or positive value |
| `CircuitBreakerFailureThreshold must be greater than 0` | Threshold is 0 or negative | Use a positive value (e.g., 5) |
| `CircuitBreakerBreakDurationSeconds must be greater than 0` | Duration is 0 or negative | Use a positive value (e.g., 30) |

### Manual Validation

For testing or manual validation scenarios:

```csharp
var options = new OnePasswordConnectOptions("my-token");
options.TimeoutSeconds = -1; // Invalid!

try
{
    options.Validate();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    // Output: Configuration validation failed: TimeoutSeconds must be greater than 0.
}
```

## Environment-Specific Configuration

### Development Configuration

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOnePasswordConnect(options =>
    {
        options.BaseUrl = "http://localhost:8080";
        options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
        options.TimeoutSeconds = 60;     // Longer timeout for debugging
        options.RetryCount = 1;          // Fewer retries for faster feedback
    });
}
```

### Production Configuration

```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddOnePasswordConnect(options =>
    {
        options.BaseUrl = "https://connect.example.com"; // HTTPS in production
        options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
        options.TimeoutSeconds = 30;                     // Standard timeout
        options.RetryCount = 3;                          // Standard retries
        options.CircuitBreakerFailureThreshold = 5;
        options.CircuitBreakerBreakDurationSeconds = 30;
    });
}
```

### Configuration from JSON

**appsettings.json:**

```json
{
  "OnePasswordConnect": {
    "BaseUrl": "http://localhost:8080",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerBreakDurationSeconds": 30
  }
}
```

**Program.cs:**

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    var config = builder.Configuration.GetSection("OnePasswordConnect");

    options.BaseUrl = config["BaseUrl"]!;
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    options.TimeoutSeconds = config.GetValue<int>("TimeoutSeconds");
    options.RetryCount = config.GetValue<int>("RetryCount");
    options.CircuitBreakerFailureThreshold = config.GetValue<int>("CircuitBreakerFailureThreshold");
    options.CircuitBreakerBreakDurationSeconds = config.GetValue<int>("CircuitBreakerBreakDurationSeconds");
});
```

### Configuration from Environment Variables

```bash
export OnePassword__BaseUrl="http://localhost:8080"
export OnePassword__ApiToken="your-token-here"
export OnePassword__TimeoutSeconds="60"
```

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = builder.Configuration["OnePassword:BaseUrl"]!;
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    options.TimeoutSeconds = builder.Configuration.GetValue<int>("OnePassword:TimeoutSeconds", 30);
});
```

## Multiple 1Password Connect Instances

If you need to connect to multiple 1Password Connect servers, you can use named HttpClient instances (advanced scenario):

```csharp
// This requires custom implementation - the SDK currently supports a single instance
// Contact the maintainer if you need this functionality
```

## Configuration Best Practices

1. ✅ **Never hardcode API tokens** - Use configuration providers
2. ✅ **Use HTTPS in production** - Only use HTTP for local development
3. ✅ **Validate configuration early** - Leverage built-in validation
4. ✅ **Environment-specific settings** - Different settings for dev/staging/prod
5. ✅ **Monitor resilience settings** - Adjust based on actual failure rates
6. ✅ **Secure configuration storage** - Use Azure Key Vault, AWS Secrets Manager, etc.
