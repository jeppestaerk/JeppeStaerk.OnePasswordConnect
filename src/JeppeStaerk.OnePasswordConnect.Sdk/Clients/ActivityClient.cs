using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Client for activity/audit log operations in the 1Password Connect API.
/// </summary>
public class ActivityClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public ActivityClient(IHttpClientFactory httpClientFactory, ILogger<ActivityClient> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <summary>
    /// Retrieves a list of API requests that have been made.
    /// </summary>
    /// <param name="limit">How many API events should be retrieved in a single request (default: 50).</param>
    /// <param name="offset">How far into the collection of API events should the response start (default: 0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of API requests.</returns>
    public async Task<List<ApiRequest>> GetApiActivityAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than 0.", nameof(limit));
        }

        if (offset < 0)
        {
            throw new ArgumentException("Offset cannot be negative.", nameof(offset));
        }

        var path = $"/v1/activity?limit={limit}&offset={offset}";
        return await GetAsync<List<ApiRequest>>(path, cancellationToken).ConfigureAwait(false);
    }
}
