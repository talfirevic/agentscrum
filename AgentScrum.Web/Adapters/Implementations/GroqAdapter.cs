using System.Text;
using System.Text.Json;
using AgentScrum.Web.Adapters.Contracts.Groq;

namespace AgentScrum.Web.Adapters.Implementations
{
    /// <summary>
    /// Provides methods to interact with the Groq API's chat endpoint.
    /// </summary>
    public class GroqAdapter : IGroqAdapter
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.groq.com/openai/v1";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroqAdapter"/> class.
        /// </summary>
        /// <param name="apiKey">The API key for authenticating requests to the Groq API.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKey"/> is null or empty.</exception>
        public GroqAdapter(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty.");

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Asynchronously creates a chat completion using the Groq API.
        /// </summary>
        /// <param name="request">The chat completion request containing messages and parameters.</param>
        /// <returns>A task that resolves to the chat completion response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="GroqApiException">Thrown if the API returns an error response.</exception>
        /// <remarks>
        /// This method sends a POST request to the /chat/completions endpoint and expects a non-streaming response.
        /// Streaming responses (when Stream is true) are not supported in this implementation.
        /// </remarks>
        public async Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorJson);
                throw new GroqApiException(errorResponse?.Error?.Message ?? "Unknown API error", errorResponse?.Error?.Type ?? "unknown_error");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);
            return result;
        }
    }
}