using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents an error detail from the API.
/// </summary>
public class ApiError
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the error type (e.g., "invalid_request_error").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the parameter related to the error (if any).
    /// </summary>
    [JsonPropertyName("param")]
    public required string Param { get; set; }

    /// <summary>
    /// Gets or sets the error code (if any).
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}