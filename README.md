# JeppeStaerk.OnePasswordConnect

<div align="center">

**Production-ready .NET SDK for 1Password Connect API**

[![NuGet](https://img.shields.io/nuget/v/JeppeStaerk.OnePasswordConnect.Sdk.svg)](https://www.nuget.org/packages/JeppeStaerk.OnePasswordConnect.Sdk/)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.1%20%7C%20.NET%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

*Strongly-typed • Fully asynchronous • Built for dependency injection*

</div>

---

## Why This SDK?

**Stop fighting with REST APIs.** This SDK transforms the 1Password Connect API into idiomatic, type-safe C# that feels natural in modern .NET applications.

### Built for Production

- **🛡️ Resilient by Default** - Automatic retries, circuit breakers, and configurable timeouts keep your app running when networks fail
- **🔒 Security First** - Never logs secrets, supports HTTPS, integrates with your existing security infrastructure
- **📊 Observable** - Structured logging with Microsoft.Extensions.Logging shows exactly what's happening
- **✅ Type-Safe** - Full IntelliSense support with strongly-typed models matching the 1Password API
- **⚡ Performance-Focused** - Async/await throughout, stream-based file downloads, efficient memory usage

### Developer Experience

```csharp
// One line to configure
builder.Services.AddOnePasswordConnect(
    baseUrl: "http://localhost:8080",
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);

// Inject and use anywhere
public class MyService
{
    public MyService(OnePasswordConnectClient client) => _client = client;

    public async Task<string> GetDatabasePassword()
    {
        var item = await _client.Items.GetVaultItemByIdAsync("vault-id", "item-id");
        return item.Fields?.First(f => f.Purpose == FieldPurpose.PASSWORD)?.Value ?? "";
    }
}
```

## Quick Start

### Installation

```bash
dotnet add package JeppeStaerk.OnePasswordConnect.Sdk
```

### Basic Setup

```csharp
using JeppeStaerk.OnePasswordConnect.Sdk.Extensions;

// In Program.cs
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;

    // Optional: Fine-tune resilience
    options.RetryCount = 3;
    options.CircuitBreakerFailureThreshold = 5;
    options.TimeoutSeconds = 30;
});
```

### Use It Anywhere

```csharp
public class SecretService
{
    private readonly OnePasswordConnectClient _client;

    public SecretService(OnePasswordConnectClient client) => _client = client;

    public async Task<string> GetApiKey(string vaultId, string itemId)
    {
        var item = await _client.Items.GetVaultItemByIdAsync(vaultId, itemId);
        var apiKeyField = item.Fields?.FirstOrDefault(f => f.Label == "api_key");
        return apiKeyField?.Value ?? throw new Exception("API key not found");
    }
}
```

## What's Included?

| Feature | Status |
|---------|--------|
| **Vaults** - List, filter, retrieve | ✅ |
| **Items** - Full CRUD + JSON Patch | ✅ |
| **Files** - Download (bytes & streams) | ✅ |
| **Activity Logs** - Audit API usage | ✅ |
| **Health Checks** - Monitor server status | ✅ |
| **Resilience** - Retry + Circuit Breaker | ✅ |
| **Logging** - Structured Microsoft.Extensions.Logging | ✅ |
| **Security** - No secrets in logs, HTTPS support | ✅ |

## Documentation

**Get Started:**
- 📖 [Getting Started Guide](docs/getting-started.md) - Installation and first steps
- 💡 [Usage Examples](docs/usage-examples.md) - Copy-paste ready examples
- ⚙️ [Configuration](docs/configuration.md) - All configuration options explained

**Production Deployment:**
- 🔒 [Security Best Practices](docs/security.md) - Handling secrets safely
- 🛡️ [Resilience & Fault Tolerance](docs/resilience.md) - Retry policies and circuit breakers
- 📊 [Logging](docs/logging.md) - Observability and debugging

**Reference:**
- 🚨 [Error Handling](docs/error-handling.md) - Exception types and handling
- 📚 [API Reference](docs/api-reference.md) - Complete API coverage
- 🔐 [Supply Chain Security](docs/supply-chain-security.md) - Package attestation and verification

## Requirements

- **.NET Standard 2.1** or **.NET 10.0+**
  - Works with .NET Core 3.0+, .NET 5+, .NET 6+, .NET 7+, .NET 8+, .NET 9+, .NET 10+
  - Also compatible with .NET Framework 4.6.2+ (via .NET Standard 2.1)
- **1Password Connect Server** (v1.8.1)
- **Microsoft.Extensions.DependencyInjection** (built-in with ASP.NET Core)

## Features at a Glance

### 🎯 Strongly Typed Everything

Work with real C# types, not magic strings:

```csharp
var newItem = new FullItem
{
    Category = ItemCategory.LOGIN,
    Fields = new List<Field>
    {
        new Field
        {
            Type = FieldType.CONCEALED,
            Purpose = FieldPurpose.PASSWORD,
            Value = "secure-password"
        }
    }
};
```

### 🔄 Automatic Resilience

The SDK automatically handles transient failures:
- **Retries with exponential backoff** (2s, 4s, 8s)
- **Circuit breaker** protects failing servers
- **Configurable timeouts** prevent hung requests
- **All configurable** to match your needs

### 📝 Built-in Logging

See exactly what's happening:

```
dbug: GET /v1/vaults/abc123 → 200 OK
warn: Retry attempt 1/3 after 2s (timeout)
fail: Circuit breaker opened for 30s (5 consecutive failures)
info: Circuit breaker reset - connection restored
```

### 🔐 Security by Design

- ✅ Never logs request/response bodies
- ✅ Never logs authorization headers
- ✅ HTTPS recommended for production
- ✅ Integrates with secure configuration (Azure Key Vault, etc.)

## Version Compatibility

This SDK targets 1Password Connect version:

- **API Version:** 1.8.1

## Contributing

Contributions are welcome! Please feel free to:
- 🐛 [Report bugs](https://github.com/jeppestaerk/JeppeStaerk.OnePasswordConnect/issues)
- 💡 [Request features](https://github.com/jeppestaerk/JeppeStaerk.OnePasswordConnect/issues)
- 🔀 [Submit pull requests](https://github.com/jeppestaerk/JeppeStaerk.OnePasswordConnect/pulls)

## License

MIT License - see [LICENSE](LICENSE) for details

## Resources

- [1Password Connect API Documentation](https://support.1password.com/connect-api-reference/)
- [1Password Connect Server](https://support.1password.com/secrets-automation/)
- [SDK Documentation](docs/)

---

<div align="center">
Made with ❤️ for the .NET community
</div>
