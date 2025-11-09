using JeppeStaerk.OnePasswordConnect.Sdk.Models;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

/// <summary>
/// Exception thrown when access is forbidden (403 Forbidden).
/// </summary>
public class ForbiddenException : OnePasswordConnectException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorResponse">The error response from the API.</param>
    public ForbiddenException(int statusCode, ErrorResponse errorResponse)
        : base(statusCode, errorResponse)
    {
    }
}
