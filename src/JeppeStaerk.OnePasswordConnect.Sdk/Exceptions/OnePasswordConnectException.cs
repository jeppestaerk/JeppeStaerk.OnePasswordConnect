using System;
using JeppeStaerk.OnePasswordConnect.Sdk.Models;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Exceptions;

/// <summary>
/// Base exception for all 1Password Connect API errors.
/// </summary>
public class OnePasswordConnectException : Exception
{
    /// <summary>
    /// HTTP status code of the error response.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// The error response from the API.
    /// </summary>
    public ErrorResponse? ErrorResponse { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public OnePasswordConnectException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public OnePasswordConnectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnePasswordConnectException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorResponse">The error response from the API.</param>
    public OnePasswordConnectException(int statusCode, ErrorResponse errorResponse)
        : base(errorResponse.Message)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }
}
