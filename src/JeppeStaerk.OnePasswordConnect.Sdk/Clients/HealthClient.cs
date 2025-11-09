using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Client for health check and monitoring operations in the 1Password Connect API.
/// </summary>
public class HealthClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public HealthClient(IHttpClientFactory httpClientFactory, ILogger<HealthClient> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <summary>
    /// Pings the server for liveness.
    /// This endpoint is designed for health checks and load balancers to verify the server is responsive.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A simple heartbeat response (typically ".").
    /// A successful response (200 OK) indicates the server is alive and accepting requests.
    /// </returns>
    /// <remarks>
    /// This is a lightweight endpoint that can be called frequently without performance impact.
    /// Ideal for Kubernetes liveness probes, load balancer health checks, and monitoring systems.
    /// </remarks>
    public async Task<string> GetHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetRawAsync("/heartbeat", cancellationToken);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStringAsync(cancellationToken);
#else
        return await response.Content.ReadAsStringAsync();
#endif
    }

    /// <summary>
    /// Gets the state of the server and its dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Server health information.</returns>
    public async Task<ServerHealth> GetServerHealthAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<ServerHealth>("/health", cancellationToken);
    }

    /// <summary>
    /// Queries the server for exposed Prometheus metrics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prometheus metrics in text format.</returns>
    public async Task<string> GetPrometheusMetricsAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetRawAsync("/metrics", cancellationToken);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStringAsync(cancellationToken);
#else
        return await response.Content.ReadAsStringAsync();
#endif
    }
}
