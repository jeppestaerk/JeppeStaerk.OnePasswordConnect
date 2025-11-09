# Error Handling

Complete guide to handling errors and exceptions when using the JeppeStaerk.OnePasswordConnect SDK.

## Table of Contents

- [Exception Types](#exception-types)
- [Error Handling Patterns](#error-handling-patterns)
- [Common Errors](#common-errors)
- [Best Practices](#best-practices)

## Exception Types

The SDK provides typed exceptions for different HTTP error codes, making it easy to handle specific error scenarios.

### Exception Hierarchy

```
OnePasswordConnectException (base class)
├── BadRequestException (400)
├── UnauthorizedException (401)
├── ForbiddenException (403)
└── NotFoundException (404)
```

### OnePasswordConnectException

**Base exception** for all 1Password Connect API errors.

**Properties:**
- `StatusCode` (HttpStatusCode) - The HTTP status code
- `ErrorResponse` (ErrorResponse?) - Structured error from the API
- `Message` (string) - Error message

```csharp
catch (OnePasswordConnectException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
    Console.WriteLine($"Status code: {ex.StatusCode}");

    if (ex.ErrorResponse != null)
    {
        Console.WriteLine($"Error: {ex.ErrorResponse.Message}");
    }
}
```

### BadRequestException (400)

Thrown when the request is malformed or invalid.

**Common causes:**
- Invalid JSON in request body
- Missing required fields
- Invalid field values
- Malformed filter queries

```csharp
catch (BadRequestException ex)
{
    Console.WriteLine($"Invalid request: {ex.Message}");
    // Fix your request data or parameters
}
```

### UnauthorizedException (401)

Thrown when authentication fails.

**Common causes:**
- Invalid API token
- Expired API token
- Missing API token

```csharp
catch (UnauthorizedException ex)
{
    Console.WriteLine("Authentication failed - check your API token");
    // Verify token is correct and not expired
    // Check token has not been revoked
}
```

### ForbiddenException (403)

Thrown when the authenticated user doesn't have permission.

**Common causes:**
- Token doesn't have access to the requested vault
- Token doesn't have permission for the operation
- Token scope is too limited

```csharp
catch (ForbiddenException ex)
{
    Console.WriteLine("Access denied - insufficient permissions");
    // Verify token has access to the vault
    // Check token permissions
}
```

### NotFoundException (404)

Thrown when the requested resource doesn't exist.

**Common causes:**
- Vault ID doesn't exist
- Item ID doesn't exist
- File ID doesn't exist
- Incorrect ID format

```csharp
catch (NotFoundException ex)
{
    Console.WriteLine("Resource not found");
    // Verify IDs are correct
    // Check resource hasn't been deleted
}
```

## Error Handling Patterns

### Basic Error Handling

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

try
{
    var item = await client.Items.GetVaultItemByIdAsync("vault-id", "item-id");
    Console.WriteLine($"Item: {item.Title}");
}
catch (NotFoundException)
{
    Console.WriteLine("Item not found");
}
catch (UnauthorizedException)
{
    Console.WriteLine("Authentication failed");
}
catch (ForbiddenException)
{
    Console.WriteLine("Access denied");
}
catch (OnePasswordConnectException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

### Specific Error Handling

Handle different scenarios with specific actions:

```csharp
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
    return item;
}
catch (UnauthorizedException ex)
{
    // Log and re-throw - this is a configuration problem
    logger.LogError(ex, "Invalid API token");
    throw;
}
catch (ForbiddenException ex)
{
    // Log and return null - user doesn't have access
    logger.LogWarning("User doesn't have access to item {ItemId}", itemId);
    return null;
}
catch (NotFoundException ex)
{
    // Return null - item doesn't exist
    logger.LogInformation("Item {ItemId} not found", itemId);
    return null;
}
catch (BadRequestException ex)
{
    // Log and throw custom exception - programming error
    logger.LogError(ex, "Invalid request parameters");
    throw new InvalidOperationException("Invalid item ID format", ex);
}
```

### Async Error Handling

```csharp
public async Task<FullItem?> GetItemSafelyAsync(string vaultId, string itemId)
{
    try
    {
        return await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
    }
    catch (NotFoundException)
    {
        return null; // Item doesn't exist
    }
    catch (OnePasswordConnectException ex)
    {
        logger.LogError(ex, "Failed to retrieve item {ItemId}", itemId);
        throw; // Re-throw other errors
    }
}
```

### Retry with Exponential Backoff

```csharp
public async Task<FullItem> GetItemWithRetryAsync(
    string vaultId,
    string itemId,
    int maxRetries = 3)
{
    int attempt = 0;

    while (true)
    {
        try
        {
            return await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
        }
        catch (NotFoundException)
        {
            // Don't retry if item doesn't exist
            throw;
        }
        catch (UnauthorizedException)
        {
            // Don't retry auth failures
            throw;
        }
        catch (OnePasswordConnectException) when (attempt < maxRetries)
        {
            attempt++;
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            logger.LogWarning("Retry {Attempt}/{MaxRetries} after {Delay}s",
                attempt, maxRetries, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}
```

### Graceful Degradation

```csharp
public async Task<string> GetDatabasePasswordAsync()
{
    try
    {
        var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
        var passwordField = item.Fields?.FirstOrDefault(f => f.Purpose == FieldPurpose.PASSWORD);
        return passwordField?.Value ?? throw new Exception("Password field not found");
    }
    catch (OnePasswordConnectException ex)
    {
        logger.LogError(ex, "Failed to retrieve password from 1Password");

        // Fall back to environment variable (not recommended for production)
        var fallback = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (!string.IsNullOrEmpty(fallback))
        {
            logger.LogWarning("Using fallback password from environment variable");
            return fallback;
        }

        throw; // No fallback available
    }
}
```

## Common Errors

### "Invalid or expired API token"

**Error:** `UnauthorizedException`

**Causes:**
- Token is incorrect
- Token has expired
- Token has been revoked

**Solutions:**
1. Verify the token in your configuration
2. Generate a new token if expired
3. Check token hasn't been revoked in 1Password Connect

```csharp
// Verify token is loaded correctly
var token = builder.Configuration["OnePassword:ApiToken"];
if (string.IsNullOrEmpty(token))
{
    throw new InvalidOperationException("OnePassword API token not configured");
}
```

### "Vault or item not found"

**Error:** `NotFoundException`

**Causes:**
- Vault ID is incorrect
- Item ID is incorrect
- Resource has been deleted
- Resource is in a different vault

**Solutions:**
1. Verify IDs are correct (check format)
2. List vaults/items to confirm they exist
3. Check for typos in IDs

```csharp
// Validate ID format before making request
if (string.IsNullOrWhiteSpace(vaultId) || vaultId.Length != 26)
{
    throw new ArgumentException("Invalid vault ID format", nameof(vaultId));
}
```

### "Access denied to resource"

**Error:** `ForbiddenException`

**Causes:**
- Token doesn't have access to the vault
- Token permissions are too restrictive
- Vault is in a different account

**Solutions:**
1. Verify token has access to the vault
2. Check token permissions in 1Password Connect
3. Use correct token for the environment

```csharp
// Handle permission errors gracefully
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
}
catch (ForbiddenException)
{
    logger.LogWarning("Token doesn't have access to vault {VaultId}", vaultId);
    // Try alternative vault or fail gracefully
}
```

### "Connection timeout"

**Error:** `HttpRequestException` or `TaskCanceledException`

**Causes:**
- Network is slow or unreliable
- 1Password Connect server is overloaded
- Timeout setting is too low

**Solutions:**
1. Increase timeout in configuration
2. Check network connectivity
3. Verify 1Password Connect server is running

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    options.TimeoutSeconds = 60; // Increase from default 30
});
```

### "Circuit breaker is open"

**Error:** `BrokenCircuitException` (from Polly)

**Causes:**
- Multiple consecutive failures
- 1Password Connect server is down
- Network issues

**Solutions:**
1. Wait for circuit breaker to reset (default: 30 seconds)
2. Check 1Password Connect server status
3. Adjust circuit breaker settings if too sensitive

```csharp
catch (BrokenCircuitException ex)
{
    logger.LogError("Circuit breaker is open - 1Password Connect is unavailable");
    // Wait and retry, or use fallback
}
```

## Best Practices

### 1. Catch Specific Exceptions First

```csharp
// ✅ Good - specific exceptions first
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
}
catch (NotFoundException ex)
{
    // Handle missing item
}
catch (ForbiddenException ex)
{
    // Handle permission denied
}
catch (OnePasswordConnectException ex)
{
    // Handle other API errors
}

// ❌ Bad - catches everything
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
}
catch (Exception ex)
{
    // Too broad - hides important error details
}
```

### 2. Don't Swallow Exceptions

```csharp
// ❌ Bad - silently swallows errors
try
{
    await client.Items.DeleteVaultItemAsync(vaultId, itemId);
}
catch
{
    // Error is hidden - hard to debug
}

// ✅ Good - logs and handles appropriately
try
{
    await client.Items.DeleteVaultItemAsync(vaultId, itemId);
}
catch (NotFoundException)
{
    logger.LogInformation("Item {ItemId} already deleted", itemId);
}
catch (OnePasswordConnectException ex)
{
    logger.LogError(ex, "Failed to delete item {ItemId}", itemId);
    throw;
}
```

### 3. Use Structured Logging

```csharp
// ✅ Good - structured logging with context
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
}
catch (OnePasswordConnectException ex)
{
    logger.LogError(ex,
        "Failed to retrieve item {ItemId} from vault {VaultId}. Status: {StatusCode}",
        itemId, vaultId, ex.StatusCode);
    throw;
}
```

### 4. Validate Input Before API Calls

```csharp
// ✅ Good - validate early
public async Task<FullItem> GetItemAsync(string vaultId, string itemId)
{
    if (string.IsNullOrWhiteSpace(vaultId))
        throw new ArgumentException("Vault ID cannot be empty", nameof(vaultId));

    if (string.IsNullOrWhiteSpace(itemId))
        throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

    try
    {
        return await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
    }
    catch (BadRequestException ex)
    {
        // This shouldn't happen if validation is correct
        logger.LogError(ex, "Unexpected BadRequestException");
        throw;
    }
}
```

### 5. Provide Context in Error Messages

```csharp
// ✅ Good - helpful error messages
try
{
    var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
}
catch (NotFoundException)
{
    throw new InvalidOperationException(
        $"Item '{itemId}' not found in vault '{vaultId}'. " +
        "Verify the IDs are correct and the item hasn't been deleted.");
}
```
