using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Client for vault operations in the 1Password Connect API.
/// </summary>
public class VaultsClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VaultsClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public VaultsClient(IHttpClientFactory httpClientFactory, ILogger<VaultsClient> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <summary>
    /// Gets all vaults.
    /// </summary>
    /// <param name="filter">
    /// Optional SCIM filter using the equality operator.
    /// <para>Example: <c>name eq "Production Vault"</c></para>
    /// <para>The filter must follow SCIM syntax. Only the 'eq' (equals) operator is supported.</para>
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of vaults.</returns>
    public async Task<List<Vault>> GetVaultsAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var path = "/v1/vaults";
        if (!string.IsNullOrWhiteSpace(filter))
        {
            path += $"?filter={Uri.EscapeDataString(filter)}";
        }

        return await GetAsync<List<Vault>>(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets vault details and metadata by ID.
    /// </summary>
    /// <param name="vaultId">The vault ID (26-character lowercase alphanumeric string).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The vault details.</returns>
    public async Task<Vault> GetVaultByIdAsync(string vaultId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultId))
        {
            throw new ArgumentException("Vault ID cannot be null or whitespace.", nameof(vaultId));
        }

        return await GetAsync<Vault>($"/v1/vaults/{Uri.EscapeDataString(vaultId)}", cancellationToken).ConfigureAwait(false);
    }
}
