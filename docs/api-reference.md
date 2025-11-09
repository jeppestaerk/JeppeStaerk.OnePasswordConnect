# API Reference

Complete reference for the JeppeStaerk.OnePasswordConnect SDK.

## Table of Contents

- [API Coverage](#api-coverage)
- [Requirements](#requirements)
- [Version Compatibility](#version-compatibility)
- [Known Limitations](#known-limitations)
- [Client Architecture](#client-architecture)

## API Coverage

The SDK provides complete coverage of the 1Password Connect API v1.8.1.

### Vaults

Access and manage 1Password vaults.

| Operation | Endpoint | Method | Supported |
|-----------|----------|--------|-----------|
| Get all vaults | `/v1/vaults` | GET | ✅ |
| Get vault by ID | `/v1/vaults/{vaultId}` | GET | ✅ |
| SCIM filtering | `/v1/vaults?filter=...` | GET | ✅ |

**Example:**
```csharp
var vaults = await client.Vaults.GetVaultsAsync();
var vault = await client.Vaults.GetVaultByIdAsync("vault-id");
var filtered = await client.Vaults.GetVaultsAsync(filter: "name eq \"Production\"");
```

### Items

Full CRUD operations for vault items, including JSON Patch support.

| Operation | Endpoint | Method | Supported |
|-----------|----------|--------|-----------|
| Get all items in vault | `/v1/vaults/{vaultId}/items` | GET | ✅ |
| Get item by ID | `/v1/vaults/{vaultId}/items/{itemId}` | GET | ✅ |
| Create item | `/v1/vaults/{vaultId}/items` | POST | ✅ |
| Update item (full) | `/v1/vaults/{vaultId}/items/{itemId}` | PUT | ✅ |
| Partial update (PATCH) | `/v1/vaults/{vaultId}/items/{itemId}` | PATCH | ✅ |
| Delete item | `/v1/vaults/{vaultId}/items/{itemId}` | DELETE | ✅ |
| SCIM filtering | `/v1/vaults/{vaultId}/items?filter=...` | GET | ✅ |

**Example:**
```csharp
// Get
var items = await client.Items.GetVaultItemsAsync("vault-id");
var item = await client.Items.GetVaultItemByIdAsync("vault-id", "item-id");

// Create
var newItem = new FullItem { Title = "New Item", /* ... */ };
var created = await client.Items.CreateVaultItemAsync("vault-id", newItem);

// Update
item.Title = "Updated";
var updated = await client.Items.UpdateVaultItemAsync("vault-id", "item-id", item);

// Patch
var patchOps = new List<PatchOperation> { /* ... */ };
var patched = await client.Items.PatchVaultItemAsync("vault-id", "item-id", patchOps);

// Delete
await client.Items.DeleteVaultItemAsync("vault-id", "item-id");
```

### Files

Download files attached to vault items.

| Operation | Endpoint | Method | Supported |
|-----------|----------|--------|-----------|
| Get all files in item | `/v1/vaults/{vaultId}/items/{itemId}/files` | GET | ✅ |
| Get file by ID | `/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}` | GET | ✅ |
| Download file content (bytes) | `/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}/content` | GET | ✅ |
| Download file content (stream) | `/v1/vaults/{vaultId}/items/{itemId}/files/{fileId}/content` | GET | ✅ |
| Inline file content | `/v1/vaults/{vaultId}/items/{itemId}/files?inline_files=true` | GET | ✅ |
| **Upload files** | - | POST | ❌ Not supported by API |

**Example:**
```csharp
// List files
var files = await client.Files.GetItemFilesAsync("vault-id", "item-id");

// Get file details
var file = await client.Files.GetFileByIdAsync("vault-id", "item-id", "file-id");

// Download as bytes
byte[] content = await client.Files.DownloadFileContentAsync("vault-id", "item-id", "file-id");

// Download as stream (recommended for large files)
using var stream = await client.Files.DownloadFileStreamAsync("vault-id", "item-id", "file-id");
```

### Activity & Audit Logs

Query API request activity and audit logs.

| Operation | Endpoint | Method | Supported |
|-----------|----------|--------|-----------|
| Get API activity | `/v1/activity` | GET | ✅ |
| Pagination support | `/v1/activity?limit={limit}&offset={offset}` | GET | ✅ |

**Example:**
```csharp
var activity = await client.Activity.GetApiActivityAsync(limit: 10, offset: 0);

foreach (var request in activity)
{
    Console.WriteLine($"[{request.Timestamp}] {request.Action} - {request.Result}");
}
```

### Health & Monitoring

Monitor server health and metrics.

| Operation | Endpoint | Method | Supported |
|-----------|----------|--------|-----------|
| Heartbeat (liveness) | `/heartbeat` | GET | ✅ |
| Health check | `/health` | GET | ✅ |
| Prometheus metrics | `/metrics` | GET | ✅ |

**Example:**
```csharp
// Liveness check
var heartbeat = await client.Health.GetHeartbeatAsync();
// Returns: "."

// Detailed health
var health = await client.Health.GetServerHealthAsync();
Console.WriteLine($"Server: {health.Name} v{health.Version}");

// Prometheus metrics
var metrics = await client.Health.GetPrometheusMetricsAsync();
```

## Requirements

### Runtime Requirements

- **.NET Standard 2.1** or **.NET 9.0+** (multi-targeted)
  - Compatible with:
    - .NET Core 3.0, 3.1
    - .NET 5.0, 6.0, 7.0, 8.0, 9.0
    - .NET Framework 4.6.2+ (via .NET Standard 2.1)
    - Mono 5.4+
    - Xamarin.iOS 10.14+
    - Xamarin.Android 8.0+

### Dependencies

- **Microsoft.Extensions.DependencyInjection** - Required for service registration
- **Microsoft.Extensions.Http** - IHttpClientFactory support
- **Microsoft.Extensions.Http.Polly** - Resilience policies (retry, circuit breaker)
- **Microsoft.Extensions.Logging.Abstractions** - Structured logging
- **Microsoft.Extensions.Options** - Options pattern
- **Polly** - Resilience framework
- **System.Text.Json** - JSON serialization

### Infrastructure Requirements

- **1Password Connect Server** (v1.8.1)
- **Valid 1Password Connect API token**
- **Network access** to 1Password Connect Server
- **HTTPS recommended** for production (HTTP OK for development)

## Version Compatibility

### SDK Version Alignment

This SDK version matches the 1Password Connect API specification version:

| SDK Version | API Version | OpenAPI Spec |
|-------------|-------------|--------------|
| 1.8.1 | 1.8.1 | `res/1password-connect-api_1.8.1.yaml` |

The SDK version will always match the API specification version to make compatibility clear.

### OpenAPI Specification

The OpenAPI specification file is included in the repository for reference:

- **Path:** `res/1password-connect-api_1.8.1.yaml`
- **Spec Version:** OpenAPI 3.0.0
- **API Version:** 1.8.1

### Breaking Changes Policy

- **Major version changes** (2.x.x) - Breaking API changes
- **Minor version changes** (1.x.x) - New features, backward compatible
- **Patch version changes** (1.8.x) - Bug fixes, backward compatible

## Known Limitations

### Dependency Injection Only

The SDK is designed **exclusively for dependency injection** and cannot be instantiated manually.

**Why:**
- `OnePasswordConnectClient` constructor requires all five specialized clients
- Each client requires `IHttpClientFactory` and `ILogger<TClient>` instances
- Manual instantiation would bypass resilience policies and logging

**Always use:**
```csharp
builder.Services.AddOnePasswordConnect(/* ... */);
```

**Don't do:**
```csharp
// ❌ Won't work - too many required dependencies
var client = new OnePasswordConnectClient(/* ... */);
```

### No File Upload Support

The 1Password Connect API (v1.8.1) **does not support file uploads** - only file downloads are available.

**You can:**
- ✅ List files attached to items
- ✅ Download file content (as bytes or stream)
- ✅ Get file metadata

**You cannot:**
- ❌ Upload new files to items
- ❌ Update existing files
- ❌ Delete files

This is a **limitation of the API itself**, not the SDK. File upload may be added in future API versions.

### No HTTP Diagnostics Package Included

The SDK intentionally does **NOT** include `Microsoft.Extensions.Http.Diagnostics` as a dependency.

**Why:**
- This is a secrets management SDK
- Logging request/response bodies could expose sensitive data (passwords, API tokens, vault contents)

**If you need HTTP diagnostics:**
1. Add the package yourself
2. Implement proper redaction for sensitive headers and bodies
3. **Never log request/response bodies**

See [Advanced HTTP Diagnostics](resilience.md#advanced-http-diagnostics) for guidance.

### Enum Naming Convention

Enums use **API naming directly** (SCREAMING_SNAKE_CASE or lowercase):

```csharp
// Enums match API naming exactly
var item = new FullItem
{
    Category = ItemCategory.LOGIN,        // Not ItemCategory.Login
    Fields = new List<Field>
    {
        new Field
        {
            Type = FieldType.CONCEALED,   // Not FieldType.Concealed
            Purpose = FieldPurpose.PASSWORD // Not FieldPurpose.Password
        }
    }
};

// Exception: JSON Patch uses lowercase per RFC6902
var patchOp = new PatchOperation
{
    Op = PatchOperation.replace,  // lowercase per standard
    Path = "/title",
    Value = "New Title"
};
```

## Client Architecture

### Main Client

`OnePasswordConnectClient` - Facade providing access to all operations.

```csharp
public class OnePasswordConnectClient
{
    public VaultsClient Vaults { get; }
    public ItemsClient Items { get; }
    public FilesClient Files { get; }
    public ActivityClient Activity { get; }
    public HealthClient Health { get; }
}
```

### Specialized Clients

Each operation type has its own client:

| Client | Purpose | Namespace |
|--------|---------|-----------|
| `VaultsClient` | Vault operations | `JeppeStaerk.OnePasswordConnect.Sdk.Clients` |
| `ItemsClient` | Item CRUD + JSON Patch | `JeppeStaerk.OnePasswordConnect.Sdk.Clients` |
| `FilesClient` | File download operations | `JeppeStaerk.OnePasswordConnect.Sdk.Clients` |
| `ActivityClient` | API activity/audit logs | `JeppeStaerk.OnePasswordConnect.Sdk.Clients` |
| `HealthClient` | Health checks, metrics | `JeppeStaerk.OnePasswordConnect.Sdk.Clients` |

### Models

All models are in `JeppeStaerk.OnePasswordConnect.Sdk.Models`:

- `Vault` - Vault information
- `FullItem` - Complete item with all fields
- `Item` - Summary item information
- `Field` - Item field (username, password, etc.)
- `File` - File metadata
- `ApiRequest` - Activity log entry
- `ServerHealth` - Server health status
- And more...

### Enums

All enums are in `JeppeStaerk.OnePasswordConnect.Sdk.Enums`:

- `ItemCategory` - LOGIN, PASSWORD, API_CREDENTIAL, etc.
- `FieldType` - STRING, CONCEALED, EMAIL, URL, etc.
- `FieldPurpose` - USERNAME, PASSWORD, NONE
- `PatchOperation` - add, remove, replace, etc.

### Exceptions

All exceptions are in `JeppeStaerk.OnePasswordConnect.Sdk.Exceptions`:

- `OnePasswordConnectException` - Base exception
- `BadRequestException` - 400 errors
- `UnauthorizedException` - 401 errors
- `ForbiddenException` - 403 errors
- `NotFoundException` - 404 errors

## Further Reading

- [Getting Started Guide](getting-started.md)
- [Usage Examples](usage-examples.md)
- [Configuration](configuration.md)
- [Security Best Practices](security.md)
- [Error Handling](error-handling.md)
