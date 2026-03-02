using Crm.Shared.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Modules.Integrations.Meta;

public class MetaModule : ICrmModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add Meta specific services (e.g. Graph API client)
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/webhooks/meta").WithTags("Meta Webhooks");

        group.MapGet("/", (HttpContext context) =>
        {
            var challenge = context.Request.Query["hub.challenge"].ToString();
            var verifyToken = context.Request.Query["hub.verify_token"].ToString();

            // Note: Should compare with env variable META_VERIFY_TOKEN
            return Results.Text(challenge);
        });

        group.MapPost("/", (HttpContext context) =>
        {
            // Process Meta webhook, verify signature, enqueue Hangfire job
            return Results.Ok();
        });
    }
}
