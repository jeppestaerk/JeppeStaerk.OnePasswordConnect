using JeppeStaerk.OnePasswordConnect.Sdk.Models;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

/// <summary>
/// Exception thrown when a resource is not found (404 Not Found).
/// </summary>
public class NotFoundException : OnePasswordConnectException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorResponse">The error response from the API.</param>
    public NotFoundException(int statusCode, ErrorResponse errorResponse)
        : base(statusCode, errorResponse)
    {
    }
}
