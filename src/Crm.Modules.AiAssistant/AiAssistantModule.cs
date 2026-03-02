using Crm.Shared.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Modules.AiAssistant;

public interface IChatProvider
{
    string ProviderName { get; }
    Task<string> GetResponseAsync(string systemContext, string messagesJson);
}

public class OpenAiProvider : IChatProvider
{
    public string ProviderName => "OpenAI";
    public Task<string> GetResponseAsync(string systemContext, string messagesJson)
    {
        // Stub for actual HTTP call to OpenAI API using OPENAI_API_KEY
        return Task.FromResult("{\"response\": \"This is a stub response from OpenAI\"}");
    }
}

public class GeminiProvider : IChatProvider
{
    public string ProviderName => "Gemini";
    public Task<string> GetResponseAsync(string systemContext, string messagesJson)
    {
        // Stub for actual HTTP call to Google Gemini using GEMINI_API_KEY
        return Task.FromResult("{\"response\": \"This is a stub response from Gemini\"}");
    }
}

public class GrokProvider : IChatProvider
{
    public string ProviderName => "Grok";
    public Task<string> GetResponseAsync(string systemContext, string messagesJson)
    {
        // Stub for actual HTTP call to xAI using GROK_API_KEY
        return Task.FromResult("{\"response\": \"This is a stub response from Grok\"}");
    }
}

public class AiChatRequest
{
    public required string Provider { get; set; }
    public required string Model { get; set; }
    public required string Context { get; set; }
    public required string Messages { get; set; }
}

public class AiAssistantModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IChatProvider, OpenAiProvider>();
        services.AddScoped<IChatProvider, GeminiProvider>();
        services.AddScoped<IChatProvider, GrokProvider>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ai").WithTags("AI Assistant");

        group.MapPost("/chat", async (AiChatRequest request, IEnumerable<IChatProvider> providers) =>
        {
            var provider = providers.FirstOrDefault(p => p.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                return Results.BadRequest($"Provider {request.Provider} not supported.");
            }

            var response = await provider.GetResponseAsync(request.Context, request.Messages).ConfigureAwait(false);

            // Should also store in AiConversations table here via DbContext/Mediator

            return Results.Ok(new { response });
        });
    }
}
