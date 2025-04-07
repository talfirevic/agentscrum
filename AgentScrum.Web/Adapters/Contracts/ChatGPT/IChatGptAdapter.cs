namespace AgentScrum.Web.Adapters.Contracts;

public interface IChatGptAdapter
{
    Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest request);
}