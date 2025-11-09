using JeppeStaerk.OnePasswordConnect.Sdk.Models;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

/// <summary>
/// Exception thrown when authentication fails (401 Unauthorized).
/// </summary>
public class UnauthorizedException : OnePasswordConnectException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorResponse">The error response from the API.</param>
    public UnauthorizedException(int statusCode, ErrorResponse errorResponse)
        : base(statusCode, errorResponse)
    {
    }
}
