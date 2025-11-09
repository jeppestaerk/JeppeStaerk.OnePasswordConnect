# Security Best Practices

This SDK handles highly sensitive data (passwords, API tokens, secrets). Follow these security best practices to protect your secrets.

## Table of Contents

- [Sensitive Data Handling](#sensitive-data-handling)
- [Network Security](#network-security)
- [API Token Management](#api-token-management)
- [File Operations Security](#file-operations-security)
- [Best Practices Checklist](#best-practices-checklist)

## Sensitive Data Handling

### API Token Storage

**⚠️ Never hardcode API tokens in source code, configuration files, or environment variables checked into source control.**

#### ❌ Wrong: Hardcoded Token

```csharp
// DON'T DO THIS
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.ApiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."; // ❌ Exposed in source code
});
```

#### ✅ Correct: Secure Configuration

```csharp
// Development: User Secrets
// dotnet user-secrets set "OnePassword:ApiToken" "your-token-here"

// Production: Azure Key Vault, AWS Secrets Manager, etc.
builder.Services.AddOnePasswordConnect(options =>
{
    options.BaseUrl = builder.Configuration["OnePassword:BaseUrl"]!;
    options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!; // ✅ From secure storage
});
```

### Memory Handling

**Important:** API tokens are stored as regular strings in .NET memory:

- Strings are **immutable** in .NET and remain in memory until garbage collected
- Strings cannot be securely zeroed out after use
- For **maximum security** in highly sensitive environments, consider:
  - Using short-lived tokens
  - Rotating tokens frequently
  - Running in isolated environments
  - Using SecureString where applicable (though this has its own limitations)

### Logging Security

The SDK implements **secure-by-default logging**:

✅ **What the SDK Logs:**
- HTTP request paths (e.g., `/v1/vaults`)
- HTTP status codes (e.g., `200`, `401`, `500`)
- Retry attempts and delays
- Circuit breaker state changes

❌ **What the SDK NEVER Logs:**
- Request bodies (would contain secrets)
- Response bodies (would contain vault data)
- Authorization headers (would contain API token)
- Field values from items
- File contents

**Warning:** If you enable HttpClient-level logging (e.g., via `Microsoft.Extensions.Http.Diagnostics`), ensure proper redaction of sensitive headers and bodies. See [Advanced HTTP Diagnostics](resilience.md#advanced-http-diagnostics).

## Network Security

### HTTPS Enforcement

**Always use HTTPS in production** to protect API tokens and secrets in transit.

#### Development (HTTP acceptable)

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOnePasswordConnect(options =>
    {
        options.BaseUrl = "http://localhost:8080"; // ✅ OK for local development
        options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    });
}
```

#### Production (HTTPS required)

```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddOnePasswordConnect(options =>
    {
        options.BaseUrl = "https://connect.example.com"; // ✅ HTTPS in production
        options.ApiToken = builder.Configuration["OnePassword:ApiToken"]!;
    });
}
```

### Certificate Validation

The SDK uses the default .NET HttpClient certificate validation:

- ✅ Valid certificates from trusted CAs work automatically
- ✅ Self-signed certificates require installation in the trust store
- ❌ **Never** disable certificate validation in production

For development with self-signed certificates:

**Option 1: Install the certificate (recommended)**
```bash
# Linux
sudo cp connect-cert.pem /usr/local/share/ca-certificates/
sudo update-ca-certificates

# macOS
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain connect-cert.pem

# Windows
certutil -addstore -f "ROOT" connect-cert.cer
```

**Option 2: Accept specific certificates (development only)**
```csharp
// DEVELOPMENT ONLY - Never use in production
if (builder.Environment.IsDevelopment())
{
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    });
}
```

## API Token Management

### Secure Token Storage Options

#### Development: User Secrets

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

#### Production: Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);

builder.Services.AddOnePasswordConnect(
    baseUrl: builder.Configuration["OnePassword:BaseUrl"]!,
    apiToken: builder.Configuration["OnePassword-ApiToken"]! // From Key Vault
);
```

#### Production: AWS Secrets Manager

```csharp
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = secret => secret.Name.StartsWith("OnePassword");
});

builder.Services.AddOnePasswordConnect(
    baseUrl: builder.Configuration["OnePassword:BaseUrl"]!,
    apiToken: builder.Configuration["OnePassword:ApiToken"]!
);
```

#### Production: Environment Variables

```bash
# Set in your deployment environment
export OnePassword__ApiToken="your-token-here"
export OnePassword__BaseUrl="https://connect.example.com"
```

```csharp
builder.Services.AddOnePasswordConnect(
    baseUrl: Environment.GetEnvironmentVariable("OnePassword__BaseUrl")!,
    apiToken: Environment.GetEnvironmentVariable("OnePassword__ApiToken")!
);
```

### Token Rotation

Regularly rotate API tokens to minimize exposure:

1. **Generate a new token** in 1Password Connect
2. **Update your secure configuration** (Key Vault, Secrets Manager, etc.)
3. **Restart your application** to load the new token
4. **Revoke the old token** after verifying the new one works

```bash
# Example with Azure Key Vault
az keyvault secret set \
    --vault-name "my-keyvault" \
    --name "OnePassword-ApiToken" \
    --value "new-token-here"
```

### Token Scope

Use the principle of least privilege:

- ✅ Create tokens with **minimum required permissions**
- ✅ Use **separate tokens** for different environments
- ✅ Use **separate tokens** for different applications
- ❌ Don't reuse tokens across environments

## File Operations Security

### Large File Downloads

The SDK provides two methods for downloading files:

#### Small Files: `DownloadFileContentAsync()`

```csharp
// ⚠️ Loads entire file into memory as byte array
byte[] fileContent = await client.Files.DownloadFileContentAsync(vaultId, itemId, fileId);

// Risk: High memory usage for large files
// Recommendation: Use only for files < 1MB
```

#### Large Files: `DownloadFileStreamAsync()`

```csharp
// ✅ Streams file content - memory efficient
using var stream = await client.Files.DownloadFileStreamAsync(vaultId, itemId, fileId);
using var fileStream = File.Create("output.bin");
await stream.CopyToAsync(fileStream);

// Recommendation: Use for all files > 1MB
```

### Secure File Storage

When saving downloaded files:

```csharp
// ✅ Save to a secure location with proper permissions
var securePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".secrets",
    "downloaded-file.bin"
);

Directory.CreateDirectory(Path.GetDirectoryName(securePath)!);

using var stream = await client.Files.DownloadFileStreamAsync(vaultId, itemId, fileId);
using var fileStream = new FileStream(securePath, FileMode.Create, FileAccess.Write, FileShare.None);
await stream.CopyToAsync(fileStream);

// Set restrictive permissions (Linux/macOS)
if (!OperatingSystem.IsWindows())
{
    File.SetUnixFileMode(securePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
}
```

## Best Practices Checklist

### Token Security
- [ ] Never hardcode API tokens in source code
- [ ] Store tokens in secure configuration (Key Vault, Secrets Manager, etc.)
- [ ] Use different tokens for dev/staging/production
- [ ] Rotate tokens regularly (every 90 days minimum)
- [ ] Revoke tokens that are no longer needed
- [ ] Monitor token usage through activity logs

### Network Security
- [ ] Use HTTPS in production environments
- [ ] Validate SSL/TLS certificates (don't disable validation)
- [ ] Use mutual TLS if required by your security policy
- [ ] Network-isolate 1Password Connect server
- [ ] Use firewall rules to restrict access

### Application Security
- [ ] Never log request/response bodies
- [ ] Never log authorization headers
- [ ] Never log field values from items
- [ ] Use streams for large file downloads
- [ ] Implement proper access controls in your application
- [ ] Use least-privilege principle for API tokens
- [ ] Clear sensitive data from memory when possible (within .NET limitations)

### Monitoring & Auditing
- [ ] Enable and monitor activity logs
- [ ] Set up alerts for suspicious activity
- [ ] Regular security audits of token usage
- [ ] Review and rotate tokens regularly
- [ ] Monitor for unusual access patterns

### Deployment Security
- [ ] Use secure configuration providers
- [ ] Encrypt configuration at rest
- [ ] Use environment variables or secrets management
- [ ] Implement proper CI/CD security
- [ ] Regular security updates to dependencies
- [ ] Use signed and verified packages (see [Supply Chain Security](supply-chain-security.md))

## Security Incident Response

If you suspect a token has been compromised:

1. **Immediately revoke** the compromised token in 1Password Connect
2. **Generate a new token** with a different value
3. **Update all applications** using the old token
4. **Review activity logs** for unauthorized access
5. **Investigate the root cause** of the compromise
6. **Implement additional security measures** to prevent recurrence

## Reporting Security Issues

If you discover a security vulnerability in this SDK:

- 🔒 **Do NOT** open a public GitHub issue
- 📧 **Email** security concerns to the maintainer
- 🛡️ **Provide details** about the vulnerability
- ⏱️ **Allow time** for a fix before public disclosure

See the repository's SECURITY.md file for the security policy.
