using JeppeStaerk.OnePasswordConnect.Sdk.Models;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

/// <summary>
/// Exception thrown when the request is invalid (400 Bad Request).
/// </summary>
public class BadRequestException : OnePasswordConnectException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public BadRequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorResponse">The error response from the API.</param>
    public BadRequestException(int statusCode, ErrorResponse errorResponse)
        : base(statusCode, errorResponse)
    {
    }
}
