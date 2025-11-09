# Logging

The SDK includes comprehensive structured logging using `Microsoft.Extensions.Logging` to help you monitor and debug your application.

## Table of Contents

- [What Gets Logged](#what-gets-logged)
- [Configuration](#configuration)
- [Log Levels](#log-levels)
- [Filtering Logs](#filtering-logs)
- [Example Log Output](#example-log-output)
- [Security](#security)

## What Gets Logged

The SDK logs all HTTP operations and resilience events with appropriate severity levels.

### Debug Level
- HTTP request method and path
- HTTP response status code and path
- Request/response metadata (NO sensitive data)

### Info Level
- Circuit breaker reset events (connection restored)
- Successful recovery after failures

### Warning Level
- HTTP 4xx errors (client errors)
- Retry attempts with delay and reason
- Recoverable failures

### Error Level
- HTTP 5xx errors (server errors)
- Circuit breaker opening events
- Non-recoverable failures
- Connection failures

## Configuration

Logging is automatically integrated when using dependency injection.

### Basic Setup

```csharp
using Microsoft.Extensions.Logging;

// Configure logging in your application
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Add 1Password Connect client
builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);
```

### Production Configuration

```csharp
builder.Services.AddLogging(logging =>
{
    // Console logging for container environments
    logging.AddConsole();

    // Structured logging with Serilog
    logging.AddSerilog();

    // Set minimum level based on environment
    if (builder.Environment.IsProduction())
    {
        logging.SetMinimumLevel(LogLevel.Information);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});
```

### With Serilog

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/onepassword-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);
```

## Log Levels

### Debug Level (Development)

Shows all HTTP operations:

```
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Sending GET request to /v1/vaults
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Received response 200 from GET /v1/vaults

dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Sending GET request to /v1/vaults/abc123/items/def456
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Received response 200 from GET /v1/vaults/abc123/items/def456
```

### Info Level (Production Baseline)

Shows recovery events:

```
info: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker reset - connection restored
```

### Warning Level

Shows retries and client errors:

```
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 1/3 after 2s delay due to: Request timeout

warn: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Received error response 404 from GET /v1/vaults/abc123/items/invalid
```

### Error Level

Shows server errors and circuit breaker events:

```
fail: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker opened for 30s due to: 5 consecutive failures

fail: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Received error response 500 from GET /v1/vaults
```

## Filtering Logs

### Filter by Namespace

```csharp
builder.Services.AddLogging(logging =>
{
    // Only show Info and above from the SDK
    logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk", LogLevel.Information);

    // Show Debug logs for a specific client
    logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient", LogLevel.Debug);
});
```

### Filter by Category

```csharp
builder.Services.AddLogging(logging =>
{
    // Show all Debug logs
    logging.SetMinimumLevel(LogLevel.Debug);

    // But silence Debug logs from the SDK
    logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk", LogLevel.Information);

    // Except for specific operations you want to debug
    logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient", LogLevel.Debug);
});
```

### Environment-Specific Logging

```csharp
builder.Services.AddLogging(logging =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Verbose logging in development
        logging.AddConsole();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Debug);
    }
    else if (builder.Environment.IsProduction())
    {
        // Structured logging in production
        logging.AddJsonConsole();
        logging.SetMinimumLevel(LogLevel.Information);

        // Filter out Debug logs
        logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk", LogLevel.Information);
    }
});
```

## Example Log Output

### Successful Request

```
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Sending GET request to /v1/vaults
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Received response 200 from GET /v1/vaults
```

### Request with Retry

```
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Sending GET request to /v1/vaults/abc123/items/def456
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 1/3 after 2s delay due to: Request timeout
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Sending GET request to /v1/vaults/abc123/items/def456
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Received response 200 from GET /v1/vaults/abc123/items/def456
```

### Circuit Breaker Event

```
fail: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker opened for 30s due to: 5 consecutive failures (InternalServerError)
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Request rejected - circuit breaker is open
      [30 seconds later]
info: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Circuit breaker reset - connection restored
```

### Client Error (404)

```
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Sending GET request to /v1/vaults/abc123/items/invalid
warn: JeppeStaerk.OnePasswordConnect.Sdk.Clients.ItemsClient[0]
      Received error response 404 from GET /v1/vaults/abc123/items/invalid
```

### Server Error (500)

```
dbug: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Sending GET request to /v1/vaults
fail: JeppeStaerk.OnePasswordConnect.Sdk.Clients.VaultsClient[0]
      Received error response 500 from GET /v1/vaults
warn: JeppeStaerk.OnePasswordConnect.Sdk.OnePasswordConnectClient[0]
      Retry attempt 1/3 after 2s delay due to: InternalServerError
```

## Security

**Important:** The SDK never logs sensitive data to protect your secrets.

### ✅ What IS Logged

- Request paths (e.g., `/v1/vaults`, `/v1/items`)
- HTTP methods (e.g., `GET`, `POST`, `PATCH`)
- Status codes (e.g., `200`, `401`, `500`)
- Retry attempts and delays
- Circuit breaker state changes

### ❌ What is NEVER Logged

- **Request bodies** (would contain secrets, passwords, field values)
- **Response bodies** (would contain vault data, item details)
- **Authorization headers** (would contain API token)
- **Field values** from items
- **File contents**
- **Any sensitive user data**

### Example: What You See vs. What is Hidden

```
✅ Logged:
dbug: Sending POST request to /v1/vaults/abc123/items
dbug: Received response 201 from POST /v1/vaults/abc123/items

❌ NOT Logged (but part of the request):
{
  "title": "Database Password",
  "fields": [
    { "label": "password", "value": "super-secret-123" }  // <- NEVER logged
  ]
}
```

## Custom Logging

### Inject Logger into Your Services

```csharp
using Microsoft.Extensions.Logging;
using JeppeStaerk.OnePasswordConnect.Sdk;

public class MyService
{
    private readonly OnePasswordConnectClient _client;
    private readonly ILogger<MyService> _logger;

    public MyService(OnePasswordConnectClient client, ILogger<MyService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string vaultId, string itemId)
    {
        _logger.LogInformation("Retrieving secret from vault {VaultId}", vaultId);

        try
        {
            var item = await _client.Items.GetVaultItemByIdAsync(vaultId, itemId);
            _logger.LogDebug("Retrieved item {ItemId}", itemId);

            // ⚠️ NEVER log the actual value!
            // _logger.LogDebug("Password: {Password}", item.Fields[0].Value); // ❌ DON'T DO THIS

            return item.Fields?.First(f => f.Purpose == FieldPurpose.PASSWORD)?.Value ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret from vault {VaultId}", vaultId);
            throw;
        }
    }
}
```

### Structured Logging Best Practices

```csharp
// ✅ Good - structured with context
_logger.LogInformation(
    "Retrieved {ItemCount} items from vault {VaultId}",
    items.Count,
    vaultId);

// ✅ Good - includes relevant IDs for correlation
_logger.LogError(ex,
    "Failed to update item {ItemId} in vault {VaultId}. Status: {StatusCode}",
    itemId, vaultId, ex.StatusCode);

// ❌ Bad - string interpolation loses structure
_logger.LogInformation($"Retrieved {items.Count} items from vault {vaultId}");

// ❌ Bad - logs sensitive data
_logger.LogDebug($"Password value: {passwordField.Value}"); // NEVER DO THIS
```

## Troubleshooting Logging

### No Logs Appearing

1. Check minimum log level:
```csharp
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug); // Lower the level
});
```

2. Check filters aren't too restrictive:
```csharp
builder.Services.AddLogging(logging =>
{
    // Remove this if it's filtering too much
    // logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk", LogLevel.None);
});
```

3. Add a logging provider:
```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // Add this if missing
});
```

### Too Many Logs

```csharp
// Reduce verbosity in production
if (builder.Environment.IsProduction())
{
    builder.Services.AddLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("JeppeStaerk.OnePasswordConnect.Sdk", LogLevel.Warning);
    });
}
```
