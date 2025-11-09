using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Client for item operations in the 1Password Connect API.
/// </summary>
public class ItemsClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemsClient(IHttpClientFactory httpClientFactory, ILogger<ItemsClient> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <summary>
    /// Gets all items from a vault.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="filter">
    /// Optional SCIM filter using the equality operator.
    /// <para>Example: <c>title eq "Database Password"</c></para>
    /// <para>The filter must follow SCIM syntax. Only the 'eq' (equals) operator is supported.</para>
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of items.</returns>
    public async Task<List<Item>> GetVaultItemsAsync(
        string vaultId,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        var path = $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items";
        if (!string.IsNullOrWhiteSpace(filter))
        {
            path += $"?filter={Uri.EscapeDataString(filter)}";
        }

        return await GetAsync<List<Item>>(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new item in a vault.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="item">The item to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created item with full details.</returns>
    public async Task<FullItem> CreateVaultItemAsync(
        string vaultId,
        FullItem item,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#else
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
#endif

        return await PostAsync<FullItem, FullItem>($"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items", item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the details of a specific item.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full item details.</returns>
    public async Task<FullItem> GetVaultItemByIdAsync(
        string vaultId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(itemId));
        }

        return await GetAsync<FullItem>($"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}", cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing item.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="item">The updated item data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated item.</returns>
    public async Task<FullItem> UpdateVaultItemAsync(
        string vaultId,
        string itemId,
        FullItem item,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(itemId));
        }

#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#else
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
#endif
        return await PutAsync<FullItem, FullItem>($"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}", item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes an item.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteVaultItemAsync(
        string vaultId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(itemId));
        }

        await DeleteAsync($"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}", cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies a JSON Patch document to partially update an item.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="patchOperations">The list of patch operations to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated item.</returns>
    public async Task<FullItem> PatchVaultItemAsync(
        string vaultId,
        string itemId,
        List<PatchOperation> patchOperations,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(itemId));
        }

        if (patchOperations == null || patchOperations.Count == 0)
        {
            throw new ArgumentException("Patch operations cannot be null or empty.", nameof(patchOperations));
        }

        return await PatchAsync<List<PatchOperation>, FullItem>(
            $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}",
            patchOperations,
            cancellationToken).ConfigureAwait(false);
    }
}
