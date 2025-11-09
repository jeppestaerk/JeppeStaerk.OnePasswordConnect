# Getting Started

This guide will help you get up and running with the JeppeStaerk.OnePasswordConnect SDK in minutes.

## Prerequisites

- **.NET Standard 2.1** or **.NET 9.0+** runtime
  - Compatible with .NET Core 3.0+, .NET 5+, .NET 6+, .NET 7+, .NET 8+, .NET 9+
  - Also works with .NET Framework 4.6.2+ via .NET Standard 2.1
- **1Password Connect Server** (v1.8.1) running locally or remotely
- **Valid 1Password Connect API token**
- **Application using dependency injection** (Microsoft.Extensions.DependencyInjection)

## Installation

Install the SDK via NuGet:

```bash
dotnet add package JeppeStaerk.OnePasswordConnect.Sdk
```

Or via the NuGet Package Manager:

```
Install-Package JeppeStaerk.OnePasswordConnect.Sdk
```

## Basic Setup

### Step 1: Add the Using Statement

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Extensions;
```

### Step 2: Configure the SDK

In your `Program.cs` (or `Startup.cs` for older .NET versions), register the SDK with dependency injection:

**Option 1: Simplified Configuration (Recommended for Getting Started)**

```csharp
builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);
```

**Option 2: Advanced Configuration (Full Control)**

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // Optional: Configure resilience settings
    options.RetryCount = 3;
    options.CircuitBreakerFailureThreshold = 5;
    options.CircuitBreakerBreakDurationSeconds = 30;
    options.TimeoutSeconds = 30;
});
```

### Step 3: Inject and Use

Inject the `OnePasswordConnectClient` into your services:

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk;

public class MyService
{
    private readonly OnePasswordConnectClient _client;

    public MyService(OnePasswordConnectClient client)
    {
        _client = client;
    }

    public async Task<string> GetSecretAsync(string vaultId, string itemId)
    {
        var item = await _client.Items.GetVaultItemByIdAsync(vaultId, itemId);
        var passwordField = item.Fields?.FirstOrDefault(f => f.Purpose == FieldPurpose.PASSWORD);
        return passwordField?.Value ?? string.Empty;
    }
}
```

## Your First Query

Here's a simple example that retrieves all vaults and lists items in the first vault:

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk;

public class VaultExplorer
{
    private readonly OnePasswordConnectClient _client;

    public VaultExplorer(OnePasswordConnectClient client)
    {
        _client = client;
    }

    public async Task ExploreVaultsAsync()
    {
        // Get all vaults
        var vaults = await _client.Vaults.GetVaultsAsync();
        Console.WriteLine($"Found {vaults.Count} vaults");

        if (vaults.Count > 0)
        {
            var firstVault = vaults[0];
            Console.WriteLine($"\nExploring vault: {firstVault.Name}");

            // Get all items in the first vault
            var items = await _client.Items.GetVaultItemsAsync(firstVault.Id!);
            Console.WriteLine($"Found {items.Count} items in vault");

            // Print item titles
            foreach (var item in items)
            {
                Console.WriteLine($"  - {item.Title} ({item.Category})");
            }
        }
    }
}
```

## Storing the API Token Securely

**Never hardcode your API token in source code.** Use one of these approaches:

### Option 1: User Secrets (Development)

```bash
dotnet user-secrets init
dotnet user-secrets set "OnePassword:ApiToken" "your-token-here"
```

```csharp
builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);
```

### Option 2: Environment Variables (Production)

```bash
export OnePassword__ApiToken="your-token-here"
```

```csharp
builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: Environment.GetEnvironmentVariable("OnePassword__ApiToken")!
);
```

### Option 3: Azure Key Vault (Production)

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword-ApiToken"]!
);
```

## Next Steps

Now that you have the SDK installed and configured, check out:

- [**Usage Examples**](usage-examples.md) - Comprehensive examples for all operations
- [**Configuration**](configuration.md) - Detailed configuration options
- [**Security Best Practices**](security.md) - How to handle secrets safely

## Troubleshooting

### "Invalid configuration" error on startup

Make sure your API token is set correctly:

```csharp
// Check if the token is null or empty
var token = builder.Configuration["OnePassword:ApiToken"];
if (string.IsNullOrEmpty(token))
{
    throw new InvalidOperationException("OnePassword API token is not configured");
}
```

### Connection timeout errors

If you're connecting to a remote server, increase the timeout:

```csharp
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "https://connect.example.com";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    options.TimeoutSeconds = 60; // Increase from default 30 seconds
});
```

### HTTPS certificate validation errors

For development with self-signed certificates, ensure your environment trusts the certificate. **Never disable certificate validation in production.**
