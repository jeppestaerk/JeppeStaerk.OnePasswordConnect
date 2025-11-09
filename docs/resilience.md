# Resilience & Fault Tolerance

The SDK includes built-in resilience policies using **Polly** to handle transient failures automatically.

## Table of Contents

- [Automatic Retry](#automatic-retry)
- [Circuit Breaker](#circuit-breaker)
- [Configuration](#configuration)
- [Advanced HTTP Diagnostics](#advanced-http-diagnostics)
- [Custom Resilience Strategies](#custom-resilience-strategies)

## Automatic Retry

The SDK automatically retries failed requests using **exponential backoff**.

### Default Behavior

- **Retry Count:** 3 attempts (configurable)
- **Backoff Strategy:** Exponential (2s, 4s, 8s)
- **Retries on:**
  - Network failures (connection errors, timeouts)
  - HTTP 5xx errors (server errors)
  - HTTP 408 (Request Timeout)

### How It Works

```
Request 1: Failed (500 Internal Server Error)
└─> Wait 2 seconds
Request 2: Failed (500 Internal Server Error)
└─> Wait 4 seconds
Request 3: Failed (500 Internal Server Error)
└─> Wait 8 seconds
Request 4: Success (200 OK)
```

### What Gets Logged

```
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 1/3 after 2s delay due to: InternalServerError

warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 2/3 after 4s delay due to: InternalServerError

warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 3/3 after 8s delay due to: InternalServerError
```

### Configure Retry Behavior

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // Customize retry count
    options.RetryCount = 5; // More retries for unreliable networks
});
```

### Disable Retries

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // Disable retries for immediate failures
    options.RetryCount = 0;
});
```

## Circuit Breaker

The circuit breaker **protects failing servers** by temporarily stopping requests after consecutive failures.

### How It Works

The circuit breaker has three states:

1. **Closed** (Normal): Requests flow through normally
2. **Open** (Broken): Requests fail immediately without hitting the server
3. **Half-Open** (Testing): One request is allowed to test if the server recovered

```
┌─────────────────────────────────────────────────┐
│ Closed (Normal)                                 │
│ ─────────────────────────────────────────       │
│ Request 1: Success (200)                        │
│ Request 2: Success (200)                        │
│ Request 3: Failed (500)                         │
│ Request 4: Failed (500)                         │
│ Request 5: Failed (500)  ← 5 failures           │
│ Request 6: Failed (500)  ← Threshold reached!   │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Open (Broken)                                   │
│ ─────────────────────────────────────────       │
│ All requests fail immediately                   │
│ Server gets time to recover                     │
│ Wait 30 seconds (configurable)                  │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Half-Open (Testing)                             │
│ ─────────────────────────────────────────       │
│ Next request: Success (200)                     │
│   → Circuit closes, normal operation resumes    │
│                OR                                │
│ Next request: Failed (500)                      │
│   → Circuit opens again for another 30s         │
└─────────────────────────────────────────────────┘
```

### Default Configuration

- **Failure Threshold:** 5 consecutive failures
- **Break Duration:** 30 seconds
- **Opens on:**
  - 5 consecutive failures (any 5xx error or network failure)
- **Closes on:**
  - First successful request after break duration

### What Gets Logged

**Circuit opens:**
```
fail: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker opened for 30s due to: 5 consecutive failures (InternalServerError)
```

**Requests while open:**
```
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Request rejected - circuit breaker is open
```

**Circuit closes (recovery):**
```
info: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker reset - connection restored
```

### Configure Circuit Breaker

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // More aggressive circuit breaker
    options.CircuitBreakerFailureThreshold = 3;  // Open after 3 failures
    options.CircuitBreakerBreakDurationSeconds = 60; // Stay open for 1 minute
});
```

## Configuration

### Default Settings (Production-Friendly)

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // These are the defaults - shown here for reference
    options.TimeoutSeconds = 30;                        // Request timeout
    options.RetryCount = 3;                             // 3 retry attempts
    options.CircuitBreakerFailureThreshold = 5;         // Open after 5 failures
    options.CircuitBreakerBreakDurationSeconds = 30;    // Stay open for 30s
});
```

### Lenient Settings (Unreliable Networks)

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // More patient configuration
    options.TimeoutSeconds = 60;                        // Longer timeout
    options.RetryCount = 5;                             // More retries
    options.CircuitBreakerFailureThreshold = 10;        // More tolerant
    options.CircuitBreakerBreakDurationSeconds = 15;    // Shorter break
});
```

### Aggressive Settings (Fail Fast)

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // Fail fast configuration
    options.TimeoutSeconds = 10;                        // Short timeout
    options.RetryCount = 1;                             // Minimal retries
    options.CircuitBreakerFailureThreshold = 2;         // Quick to open
    options.CircuitBreakerBreakDurationSeconds = 60;    // Longer recovery
});
```

### Minimal Resilience (Development)

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOnePasswordConnect(options =>
    {
        options.BaseUrl = "http://localhost:8080";
        options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

        // Faster feedback during development
        options.TimeoutSeconds = 10;
        options.RetryCount = 0; // No retries
        // Circuit breaker still active with default settings
    });
}
```

## Advanced HTTP Diagnostics

For enhanced observability and debugging, you can add HTTP diagnostics. **Important: This SDK handles sensitive data, so proper redaction is critical!**

### Adding Diagnostics (Optional)

```bash
dotnet add package Microsoft.Extensions.Http.Diagnostics
```

```csharp
using Microsoft.Extensions.Http.Diagnostics;

builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
});

// Configure diagnostics with proper redaction
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();

    // Optional: adds request hedging for reliability
    http.AddStandardHedging();
});
```

### ⚠️ Security Warning

**Never log request or response bodies** when using diagnostics with this SDK. The bodies contain sensitive secrets, passwords, and vault data.

**Always ensure:**
- ✅ Authorization headers are redacted
- ✅ Request bodies are never logged (contain secrets)
- ✅ Response bodies are never logged (contain secrets)
- ✅ Only metrics and duration logging, not content

If you need diagnostics, use **metrics and duration logging only**, not content logging.

### Example: Safe Diagnostics Configuration

```csharp
builder.Services.AddHttpLogging(logging =>
{
    // Log only request/response headers and timing
    logging.LoggingFields = HttpLoggingFields.RequestProperties
                          | HttpLoggingFields.ResponsePropertiesAndHeaders
                          | HttpLoggingFields.Duration;

    // Redact Authorization header
    logging.RequestHeaders.Add("Authorization");

    // NEVER log bodies
    logging.LoggingFields &= ~HttpLoggingFields.RequestBody;
    logging.LoggingFields &= ~HttpLoggingFields.ResponseBody;
});
```

## Custom Resilience Strategies

### Combining SDK Resilience with Custom Policies

The SDK's resilience policies run automatically. You can add additional policies at the application level:

```csharp
public class ResilientVaultService
{
    private readonly OnePasswordConnectClient _client;
    private readonly IAsyncPolicy _fallbackPolicy;

    public ResilientVaultService(OnePasswordConnectClient client)
    {
        _client = client;

        // Additional fallback policy
        _fallbackPolicy = Policy
            .Handle<OnePasswordConnectException>()
            .FallbackAsync(async ct =>
            {
                // Fallback to cached data or secondary source
                return await GetFallbackVaultsAsync(ct);
            });
    }

    public async Task<List<Vault>> GetVaultsAsync(CancellationToken ct = default)
    {
        return await _fallbackPolicy.ExecuteAsync(async () =>
        {
            // SDK retry and circuit breaker apply here
            return await _client.Vaults.GetVaultsAsync(cancellationToken: ct);
        });
    }

    private async Task<List<Vault>> GetFallbackVaultsAsync(CancellationToken ct)
    {
        // Return cached data or empty list
        return new List<Vault>();
    }
}
```

### Health Checks Integration

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

builder.Services.AddHealthChecks()
    .AddCheck("1password-connect", () =>
    {
        try
        {
            var client = serviceProvider.GetRequiredService<OnePasswordConnectClient>();
            var heartbeat = await client.Health.GetHeartbeatAsync();

            if (heartbeat == ".")
            {
                return HealthCheckResult.Healthy("1Password Connect is responsive");
            }

            return HealthCheckResult.Degraded("Unexpected heartbeat response");
        }
        catch (BrokenCircuitException)
        {
            return HealthCheckResult.Unhealthy("Circuit breaker is open");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("1Password Connect is not responding", ex);
        }
    },
    tags: new[] { "ready", "external" });
```

## Monitoring Resilience

### Track Retry Events

```csharp
public class ResilientItemService
{
    private readonly OnePasswordConnectClient _client;
    private readonly ILogger<ResilientItemService> _logger;
    private readonly IMetrics _metrics;

    public async Task<FullItem> GetItemAsync(string vaultId, string itemId)
    {
        try
        {
            var item = await _client.Items.GetVaultItemByIdAsync(vaultId, itemId);
            _metrics.Increment("onepassword.requests.success");
            return item;
        }
        catch (BrokenCircuitException)
        {
            _metrics.Increment("onepassword.circuit_breaker.open");
            throw;
        }
        catch (OnePasswordConnectException ex)
        {
            _metrics.Increment("onepassword.requests.failure");
            throw;
        }
    }
}
```

### Circuit Breaker Metrics

Monitor these metrics in production:

- **Circuit breaker opens:** Count of circuit breaker open events
- **Circuit breaker closes:** Count of recovery events
- **Retry attempts:** Count of retries per request
- **Request failures:** Total failed requests after all retries

## Best Practices

1. ✅ **Use default settings** for most scenarios
2. ✅ **Monitor retry rates** and adjust if needed
3. ✅ **Log circuit breaker events** for alerting
4. ✅ **Test resilience** with network failures
5. ✅ **Health check integration** for orchestrators (Kubernetes, etc.)
6. ✅ **Don't disable retries** unless absolutely necessary
7. ❌ **Don't make timeout too long** (blocks threads)
8. ❌ **Don't log sensitive data** in custom diagnostics
