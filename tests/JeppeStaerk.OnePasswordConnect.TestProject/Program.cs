using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JeppeStaerk.OnePasswordConnect.Sdk;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;
using JeppeStaerk.OnePasswordConnect.Sdk.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

// Get 1Password Connect configuration from environment variables
var baseUrl = builder.Configuration["ONEPASSWORD_CONNECT_URL"] ?? "http://localhost:8080";
var apiToken = builder.Configuration["ONEPASSWORD_CONNECT_TOKEN"] ?? throw new InvalidOperationException("ONEPASSWORD_CONNECT_TOKEN environment variable is required");

// Register 1Password Connect SDK
builder.Services.AddOnePasswordConnect(baseUrl, apiToken);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var host = builder.Build();

// Create a scope to resolve services
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    var client = services.GetRequiredService<OnePasswordConnectClient>();

    // Test 1: List all vaults
    logger.LogInformation("Fetching all vaults...");
    var vaults = await client.Vaults.GetVaultsAsync();
    logger.LogInformation("Found {Count} vault(s)", vaults.Count);

    foreach (var vault in vaults)
    {
        logger.LogInformation("  Vault: {Name} (ID: {Id})", vault.Name, vault.Id);
        logger.LogInformation("    Description: {Description}", vault.Description ?? "N/A");
        logger.LogInformation("    Type: {Type}", vault.Type);
        logger.LogInformation("    Items: {ItemCount}", vault.Items ?? 0);
    }

    // Test 2: If there are vaults, list items from the first one
    if (vaults.Count > 0)
    {
        var firstVault = vaults[0];
        logger.LogInformation("Fetching items from vault '{VaultName}'...", firstVault.Name);

        ArgumentException.ThrowIfNullOrWhiteSpace(firstVault.Id);
        var items = await client.Items.GetVaultItemsAsync(firstVault.Id);
        logger.LogInformation("Found {Count} item(s) in vault '{VaultName}'", items.Count, firstVault.Name);

        foreach (var item in items)
        {
            logger.LogInformation("  Item: {Title} (Category: {Category})", item.Title, item.Category);
            logger.LogInformation("    ID: {Id}", item.Id);
            logger.LogInformation("    Tags: {Tags}", item.Tags != null && item.Tags.Any() ? string.Join(", ", item.Tags) : "None");

            if (item.Urls != null && item.Urls.Any())
            {
                foreach (var url in item.Urls)
                {
                    logger.LogInformation("    URL: {Url}", url.Href);
                }
            }
        }

        // Test 3: If there are items, get detailed info for the first one
        if (items.Count > 0)
        {
            var firstItem = items[0];
            logger.LogInformation("Fetching detailed information for item '{ItemTitle}'...", firstItem.Title);

            ArgumentException.ThrowIfNullOrWhiteSpace(firstItem.Id);
            var itemDetails = await client.Items.GetVaultItemByIdAsync(firstVault.Id, firstItem.Id);
            logger.LogInformation("Item details retrieved successfully");
            logger.LogInformation("  Title: {Title}", itemDetails.Title);
            logger.LogInformation("  Category: {Category}", itemDetails.Category);
            logger.LogInformation("  Created: {Created}", itemDetails.CreatedAt);
            logger.LogInformation("  Updated: {Updated}", itemDetails.UpdatedAt);

            if (itemDetails.Fields != null && itemDetails.Fields.Any())
            {
                logger.LogInformation("  Fields ({Count}):", itemDetails.Fields.Count);
                foreach (var field in itemDetails.Fields)
                {
                    // Don't log sensitive field values
                    var valueDisplay = field.Type == FieldType.CONCEALED ? "***" : (field.Value ?? "N/A");
                    logger.LogInformation("    {Label} ({Type}): {Value}",
                        field.Label ?? field.Id,
                        field.Type,
                        valueDisplay);
                }
            }
        }
    }

    logger.LogInformation("1Password Connect SDK integration test completed successfully!");
    return 0;
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during the integration test");
    return 1;
}