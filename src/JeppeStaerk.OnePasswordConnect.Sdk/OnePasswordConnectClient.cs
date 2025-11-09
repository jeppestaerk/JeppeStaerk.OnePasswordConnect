using System;
using JeppeStaerk.OnePasswordConnect.Sdk.Clients;

namespace JeppeStaerk.OnePasswordConnect.Sdk;

/// <summary>
/// Main client for interacting with the 1Password Connect API.
/// Provides access to all API resources through specialized sub-clients.
/// </summary>
/// <remarks>
/// This client should be registered via dependency injection using AddOnePasswordConnect().
/// All HttpClient instances are managed by IHttpClientFactory for optimal performance.
/// </remarks>
public class OnePasswordConnectClient
{
    /// <summary>
    /// Client for vault operations.
    /// </summary>
    public VaultsClient Vaults { get; }

    /// <summary>
    /// Client for item operations.
    /// </summary>
    public ItemsClient Items { get; }

    /// <summary>
    /// Client for file operations.
    /// </summary>
    public FilesClient Files { get; }

    /// <summary>
    /// Client for activity/audit log operations.
    /// </summary>
    public ActivityClient Activity { get; }

    /// <summary>
    /// Client for health check and monitoring operations.
    /// </summary>
    public HealthClient Health { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectClient"/> class.
    /// This constructor is used by dependency injection and receives pre-configured typed clients.
    /// </summary>
    /// <param name="vaults">The vaults client.</param>
    /// <param name="items">The items client.</param>
    /// <param name="files">The files client.</param>
    /// <param name="activity">The activity client.</param>
    /// <param name="health">The health client.</param>
    public OnePasswordConnectClient(
        VaultsClient vaults,
        ItemsClient items,
        FilesClient files,
        ActivityClient activity,
        HealthClient health)
    {
        Vaults = vaults ?? throw new ArgumentNullException(nameof(vaults));
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Files = files ?? throw new ArgumentNullException(nameof(files));
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Health = health ?? throw new ArgumentNullException(nameof(health));
    }
}
