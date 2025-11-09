using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using File = JeppeStaerk.OnePasswordConnect.Sdk.Models.File;
using Models_File = JeppeStaerk.OnePasswordConnect.Sdk.Models.File;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Clients;

/// <summary>
/// Client for file operations in the 1Password Connect API.
/// </summary>
public class FilesClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public FilesClient(IHttpClientFactory httpClientFactory, ILogger<FilesClient> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <summary>
    /// Gets all files attached to an item.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="inlineFiles">If true, includes base64-encoded file contents in the response (for small files).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of files.</returns>
    public async Task<List<File>> GetItemFilesAsync(
        string vaultId,
        string itemId,
        bool inlineFiles = false,
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

        var path = $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}/files";
        if (inlineFiles)
        {
            path += "?inline_files=true";
        }

        return await GetAsync<List<File>>(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the details of a specific file.
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="fileId">The file ID.</param>
    /// <param name="inlineFiles">If true, includes base64-encoded file contents in the response (for small files).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file details.</returns>
    public async Task<File> GetFileByIdAsync(
        string vaultId,
        string itemId,
        string fileId,
        bool inlineFiles = false,
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

        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be null or whitespace.", nameof(fileId));
        }

        var path = $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}/files/{Uri.EscapeDataString(fileId)}";
        if (inlineFiles)
        {
            path += "?inline_files=true";
        }

        return await GetAsync<File>(path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads the content of a file as a byte array.
    /// <para>
    /// <strong>Warning:</strong> This method loads the entire file into memory. For large files (> 1MB),
    /// consider using <see cref="DownloadFileStreamAsync"/> to avoid memory pressure.
    /// </para>
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="fileId">The file ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as a byte array.</returns>
    public async Task<byte[]> DownloadFileContentAsync(
        string vaultId,
        string itemId,
        string fileId,
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

        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be null or whitespace.", nameof(fileId));
        }

        var path = $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}/files/{Uri.EscapeDataString(fileId)}/content";
        var response = await GetRawAsync(path, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Downloads the content of a file as a stream.
    /// <para>
    /// <strong>Important:</strong> The returned stream must be consumed immediately or copied to another stream.
    /// The stream is backed by the HTTP response and will become invalid if the response is disposed.
    /// It is recommended to copy the stream to a <see cref="MemoryStream"/> or file if you need to access it later.
    /// </para>
    /// </summary>
    /// <param name="vaultId">The vault ID.</param>
    /// <param name="itemId">The item ID.</param>
    /// <param name="fileId">The file ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream containing the file content.</returns>
    /// <example>
    /// <code>
    /// // Copy to a file immediately
    /// using var stream = await client.Files.DownloadFileStreamAsync(vaultId, itemId, fileId);
    /// using var fileStream = File.Create("output.dat");
    /// await stream.CopyToAsync(fileStream);
    /// </code>
    /// </example>
    public async Task<Stream> DownloadFileStreamAsync(
        string vaultId,
        string itemId,
        string fileId,
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

        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be null or whitespace.", nameof(fileId));
        }

        var path = $"/v1/vaults/{Uri.EscapeDataString(vaultId)}/items/{Uri.EscapeDataString(itemId)}/files/{Uri.EscapeDataString(fileId)}/content";
        var response = await GetRawAsync(path, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
    }
}
