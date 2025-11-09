# Usage Examples

Complete, copy-paste ready examples for all SDK operations.

## Table of Contents

- [Working with Vaults](#working-with-vaults)
- [Working with Items](#working-with-items)
- [Partial Updates with JSON Patch](#partial-updates-with-json-patch)
- [Working with Files](#working-with-files)
- [Activity & Audit Logs](#activity--audit-logs)
- [Health Checks](#health-checks)

## Working with Vaults

### Get All Vaults

```csharp
var vaults = await client.Vaults.GetVaultsAsync();

foreach (var vault in vaults)
{
    Console.WriteLine($"Vault: {vault.Name} (ID: {vault.Id})");
    Console.WriteLine($"  Items: {vault.Items}");
    Console.WriteLine($"  Type: {vault.Type}");
}
```

### Filter Vaults by Name

```csharp
// Use SCIM filter syntax
var vaults = await client.Vaults.GetVaultsAsync(
    filter: "name eq \"Production Secrets\""
);

if (vaults.Count > 0)
{
    Console.WriteLine($"Found vault: {vaults[0].Name}");
}
```

### Get a Specific Vault by ID

```csharp
var vault = await client.Vaults.GetVaultByIdAsync("vault-id-here");

Console.WriteLine($"Vault: {vault.Name}");
Console.WriteLine($"Description: {vault.Description}");
Console.WriteLine($"Number of items: {vault.Items}");
```

## Working with Items

### Get All Items in a Vault

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;

var items = await client.Items.GetVaultItemsAsync("vault-id");

foreach (var item in items)
{
    Console.WriteLine($"{item.Title} ({item.Category})");
}
```

### Filter Items by Title

```csharp
var items = await client.Items.GetVaultItemsAsync(
    "vault-id",
    filter: "title eq \"Database Password\""
);
```

### Get a Specific Item

```csharp
var item = await client.Items.GetVaultItemByIdAsync("vault-id", "item-id");

Console.WriteLine($"Title: {item.Title}");
Console.WriteLine($"Category: {item.Category}");
Console.WriteLine($"Tags: {string.Join(", ", item.Tags ?? new List<string>())}");

// Access fields
foreach (var field in item.Fields ?? Enumerable.Empty<Field>())
{
    Console.WriteLine($"  {field.Label}: {field.Value}");
}
```

### Create a New Login Item

```csharp
var newItem = new FullItem
{
    Title = "My New Login",
    Category = ItemCategory.LOGIN,
    Vault = new VaultReference { Id = "vault-id" },
    Fields = new List<Field>
    {
        new Field
        {
            Id = "username",
            Type = FieldType.STRING,
            Purpose = FieldPurpose.USERNAME,
            Label = "username",
            Value = "myuser@example.com"
        },
        new Field
        {
            Id = "password",
            Type = FieldType.CONCEALED,
            Purpose = FieldPurpose.PASSWORD,
            Label = "password",
            Value = "super-secret-password"
        }
    },
    Urls = new List<ItemUrl>
    {
        new ItemUrl
        {
            Href = "https://example.com",
            Primary = true
        }
    },
    Tags = new List<string> { "production", "web" }
};

var createdItem = await client.Items.CreateVaultItemAsync("vault-id", newItem);
Console.WriteLine($"Created item with ID: {createdItem.Id}");
```

### Create an API Credential

```csharp
var apiCredential = new FullItem
{
    Title = "GitHub API Token",
    Category = ItemCategory.API_CREDENTIAL,
    Vault = new VaultReference { Id = "vault-id" },
    Fields = new List<Field>
    {
        new Field
        {
            Label = "token",
            Type = FieldType.CONCEALED,
            Value = "ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
        },
        new Field
        {
            Label = "scope",
            Type = FieldType.STRING,
            Value = "repo, workflow"
        }
    }
};

var created = await client.Items.CreateVaultItemAsync("vault-id", apiCredential);
```

### Update an Existing Item

```csharp
// Get the item first
var item = await client.Items.GetVaultItemByIdAsync("vault-id", "item-id");

// Modify it
item.Title = "Updated Title";
item.Tags = new List<string> { "updated", "production" };

// Update the item
var updatedItem = await client.Items.UpdateVaultItemAsync(
    "vault-id",
    "item-id",
    item
);

Console.WriteLine($"Updated: {updatedItem.Title}");
```

### Delete an Item

```csharp
await client.Items.DeleteVaultItemAsync("vault-id", "item-id");
Console.WriteLine("Item deleted successfully");
```

## Partial Updates with JSON Patch

JSON Patch allows you to update specific fields without sending the entire item.

### Update the Favorite Status

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Models;

var patchOps = new List<PatchOperation>
{
    new PatchOperation
    {
        Op = Enums.PatchOperation.replace,
        Path = "/favorite",
        Value = true
    }
};

var updatedItem = await client.Items.PatchVaultItemAsync(
    "vault-id",
    "item-id",
    patchOps
);
```

### Add a New Field

```csharp
var addFieldOps = new List<PatchOperation>
{
    new PatchOperation
    {
        Op = Enums.PatchOperation.add,
        Path = "/fields/-", // Append to array
        Value = new Field
        {
            Label = "API Key",
            Type = FieldType.CONCEALED,
            Value = "my-api-key-value"
        }
    }
};

await client.Items.PatchVaultItemAsync("vault-id", "item-id", addFieldOps);
```

### Update a Specific Field Value

```csharp
var updateFieldOps = new List<PatchOperation>
{
    new PatchOperation
    {
        Op = Enums.PatchOperation.replace,
        Path = "/fields/0/value", // Update first field's value
        Value = "new-value-here"
    }
};

await client.Items.PatchVaultItemAsync("vault-id", "item-id", updateFieldOps);
```

### Add a Tag

```csharp
var addTagOps = new List<PatchOperation>
{
    new PatchOperation
    {
        Op = Enums.PatchOperation.add,
        Path = "/tags/-",
        Value = "production"
    }
};

await client.Items.PatchVaultItemAsync("vault-id", "item-id", addTagOps);
```

### Remove a Field

```csharp
var removeFieldOps = new List<PatchOperation>
{
    new PatchOperation
    {
        Op = Enums.PatchOperation.remove,
        Path = "/fields/2" // Remove the third field (zero-indexed)
    }
};

await client.Items.PatchVaultItemAsync("vault-id", "item-id", removeFieldOps);
```

## Working with Files

### Get All Files in an Item

```csharp
var files = await client.Files.GetItemFilesAsync("vault-id", "item-id");

foreach (var file in files)
{
    Console.WriteLine($"File: {file.Name}");
    Console.WriteLine($"  Size: {file.Size} bytes");
    Console.WriteLine($"  Content Path: {file.ContentPath}");
}
```

### Get Files with Inline Content (Small Files)

```csharp
var filesWithContent = await client.Files.GetItemFilesAsync(
    "vault-id",
    "item-id",
    inlineFiles: true
);

foreach (var file in filesWithContent)
{
    if (file.Content != null)
    {
        Console.WriteLine($"File {file.Name} content available inline");
    }
}
```

### Get a Specific File

```csharp
var file = await client.Files.GetFileByIdAsync("vault-id", "item-id", "file-id");

Console.WriteLine($"Filename: {file.Name}");
Console.WriteLine($"Size: {file.Size} bytes");
Console.WriteLine($"Section: {file.Section?.Id}");
```

### Download File Content as Byte Array

```csharp
byte[] fileContent = await client.Files.DownloadFileContentAsync(
    "vault-id",
    "item-id",
    "file-id"
);

// Save to disk
await File.WriteAllBytesAsync("downloaded-file.bin", fileContent);
Console.WriteLine($"Downloaded {fileContent.Length} bytes");
```

### Download File as Stream (Recommended for Large Files)

```csharp
using var stream = await client.Files.DownloadFileStreamAsync(
    "vault-id",
    "item-id",
    "file-id"
);

using var fileStream = File.Create("downloaded-file.txt");
await stream.CopyToAsync(fileStream);

Console.WriteLine("File downloaded successfully");
```

### Download and Process File Content

```csharp
using var stream = await client.Files.DownloadFileStreamAsync(
    "vault-id",
    "item-id",
    "file-id"
);

using var reader = new StreamReader(stream);
var content = await reader.ReadToEndAsync();

Console.WriteLine($"File content:\n{content}");
```

## Activity & Audit Logs

### Get Recent API Activity

```csharp
var activity = await client.Activity.GetApiActivityAsync(limit: 10, offset: 0);

Console.WriteLine($"Recent API activity ({activity.Count} requests):\n");

foreach (var request in activity)
{
    Console.WriteLine($"[{request.Timestamp}] {request.Action} - {request.Result}");
    Console.WriteLine($"  Actor: {request.Actor?.UserAgent}");
    Console.WriteLine($"  IP: {request.Actor?.RequestIp}");
    Console.WriteLine($"  Resource: {request.Resource?.Type} {request.Resource?.Vault?.Id}");
    Console.WriteLine();
}
```

### Paginate Through Activity Logs

```csharp
int pageSize = 50;
int currentOffset = 0;
bool hasMore = true;

while (hasMore)
{
    var activity = await client.Activity.GetApiActivityAsync(
        limit: pageSize,
        offset: currentOffset
    );

    if (activity.Count == 0)
    {
        hasMore = false;
        break;
    }

    foreach (var request in activity)
    {
        Console.WriteLine($"{request.Timestamp}: {request.Action}");
    }

    currentOffset += pageSize;
    hasMore = activity.Count == pageSize;
}
```

## Health Checks

### Check if Server is Alive (Heartbeat)

```csharp
var heartbeat = await client.Health.GetHeartbeatAsync();
Console.WriteLine($"Heartbeat response: {heartbeat}");
// Expected output: "."
```

### Get Server Health Status

```csharp
var health = await client.Health.GetServerHealthAsync();

Console.WriteLine($"Server: {health.Name} v{health.Version}");

if (health.Dependencies != null)
{
    Console.WriteLine("\nDependencies:");
    foreach (var dep in health.Dependencies)
    {
        Console.WriteLine($"  {dep.Service}: {dep.Status}");
        if (!string.IsNullOrEmpty(dep.Message))
        {
            Console.WriteLine($"    Message: {dep.Message}");
        }
    }
}
```

### Get Prometheus Metrics

```csharp
var metrics = await client.Health.GetPrometheusMetricsAsync();

Console.WriteLine("Prometheus Metrics:");
Console.WriteLine(metrics);

// Parse specific metrics if needed
var lines = metrics.Split('\n');
foreach (var line in lines)
{
    if (line.StartsWith("onepassword_connect"))
    {
        Console.WriteLine(line);
    }
}
```

### Health Check Integration (ASP.NET Core)

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("1password-connect", async () =>
    {
        try
        {
            var client = serviceProvider.GetRequiredService<OnePasswordConnectClient>();
            await client.Health.GetHeartbeatAsync();
            return HealthCheckResult.Healthy("1Password Connect is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("1Password Connect is not responding", ex);
        }
    });
```

## Advanced Patterns

### Retry on Specific Errors

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

async Task<FullItem> GetItemWithRetryAsync(string vaultId, string itemId)
{
    int maxAttempts = 3;
    int attempt = 0;

    while (attempt < maxAttempts)
    {
        try
        {
            return await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
        }
        catch (NotFoundException)
        {
            // Don't retry on not found
            throw;
        }
        catch (UnauthorizedException)
        {
            // Don't retry on auth failures
            throw;
        }
        catch (OnePasswordConnectException) when (attempt < maxAttempts - 1)
        {
            attempt++;
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }

    throw new Exception("Failed after maximum retry attempts");
}
```

### Bulk Operations

```csharp
async Task<Dictionary<string, FullItem>> GetMultipleItemsAsync(
    string vaultId,
    IEnumerable<string> itemIds)
{
    var tasks = itemIds.Select(async itemId =>
    {
        try
        {
            var item = await client.Items.GetVaultItemByIdAsync(vaultId, itemId);
            return (itemId, item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get item {itemId}: {ex.Message}");
            return (itemId, null);
        }
    });

    var results = await Task.WhenAll(tasks);

    return results
        .Where(r => r.item != null)
        .ToDictionary(r => r.itemId, r => r.item!);
}
```

### Cache Item Lookups

```csharp
using Microsoft.Extensions.Caching.Memory;

public class CachedItemService
{
    private readonly OnePasswordConnectClient _client;
    private readonly IMemoryCache _cache;

    public CachedItemService(OnePasswordConnectClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<FullItem> GetItemAsync(string vaultId, string itemId)
    {
        var cacheKey = $"item:{vaultId}:{itemId}";

        if (_cache.TryGetValue(cacheKey, out FullItem? cachedItem) && cachedItem != null)
        {
            return cachedItem;
        }

        var item = await _client.Items.GetVaultItemByIdAsync(vaultId, itemId);

        _cache.Set(cacheKey, item, TimeSpan.FromMinutes(5));

        return item;
    }
}
```
