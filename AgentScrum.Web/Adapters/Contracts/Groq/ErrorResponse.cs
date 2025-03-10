using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents an error response from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    [JsonPropertyName("error")]
    public ApiError Error { get; set; }
}