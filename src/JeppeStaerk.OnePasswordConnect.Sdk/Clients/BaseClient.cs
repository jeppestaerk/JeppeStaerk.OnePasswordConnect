using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Base client for all 1Password Connect API operations.
/// </summary>
public abstract class BaseClient
{
    /// <summary>
    /// The HTTP client factory for creating clients.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger Logger;

    /// <summary>
    /// JSON serialization options.
    /// </summary>
    /// <remarks>
    /// Uses global JsonStringEnumConverter to serialize/deserialize enums as strings.
    /// All enums in this SDK use values that match the 1Password Connect API naming convention,
    /// so a simple string converter works perfectly (e.g., VaultType.USER_CREATED serializes to "USER_CREATED").
    /// </remarks>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Creates a new HttpClient instance configured for 1Password Connect API.
    /// </summary>
    /// <returns>A configured HttpClient instance.</returns>
    private HttpClient CreateHttpClient() => _httpClientFactory.CreateClient(typeof(BaseClient).FullName!);

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    protected BaseClient(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    protected async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending GET request to {Path}", path);
        var response = await httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from GET {Path}", (int)response.StatusCode, path);
        return await HandleResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a GET request and returns the raw response.
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> GetRawAsync(string path, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending GET request (raw) to {Path}", path);
        var response = await httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from GET (raw) {Path}", (int)response.StatusCode, path);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a POST request with a JSON body.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="body">The request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending POST request to {Path}", path);
        var content = JsonContent.Create(body, options: JsonOptions);

        var response = await httpClient.PostAsync(path, content, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from POST {Path}", (int)response.StatusCode, path);
        return await HandleResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PUT request with a JSON body.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="body">The request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending PUT request to {Path}", path);
        var content = JsonContent.Create(body, options: JsonOptions);

        var response = await httpClient.PutAsync(path, content, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from PUT {Path}", (int)response.StatusCode, path);
        return await HandleResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PATCH request with a JSON body.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="body">The request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    protected async Task<TResponse> PatchAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending PATCH request to {Path}", path);
        var content = JsonContent.Create(body, options: JsonOptions);

        var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = content };
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from PATCH {Path}", (int)response.StatusCode, path);

        return await HandleResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient();
        Logger.LogDebug("Sending DELETE request to {Path}", path);
        var response = await httpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Received response {StatusCode} from DELETE {Path}", (int)response.StatusCode, path);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the HTTP response and deserializes it.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new OnePasswordConnectException("Failed to deserialize response.");
    }

    /// <summary>
    /// Ensures the HTTP response indicates success, throwing appropriate exceptions if not.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var statusCode = (int)response.StatusCode;
        ErrorResponse? errorResponse = null;

        try
        {
#if NET5_0_OR_GREATER
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
            errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            // Log the deserialization error for debugging, but continue with generic message
            Logger.LogDebug(ex, "Failed to deserialize error response from API");
        }

        errorResponse ??= new ErrorResponse
        {
            Status = statusCode,
            Message = $"HTTP {statusCode}: {response.ReasonPhrase ?? "Unknown error"}"
        };

        // Log the error with appropriate level
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        Logger.Log(logLevel, "API request failed with status {StatusCode}: {ErrorMessage}",
            statusCode, errorResponse.Message);

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(statusCode, errorResponse),
            HttpStatusCode.Unauthorized => new UnauthorizedException(statusCode, errorResponse),
            HttpStatusCode.Forbidden => new ForbiddenException(statusCode, errorResponse),
            HttpStatusCode.NotFound => new NotFoundException(statusCode, errorResponse),
            _ => new OnePasswordConnectException(statusCode, errorResponse)
        };
    }
}
