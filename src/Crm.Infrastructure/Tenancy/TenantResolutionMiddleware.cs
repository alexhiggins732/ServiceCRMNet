using System.Security.Claims;
using Crm.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Tenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        Guid? tenantId = null;

        // Try header first (for dev/admin overrides)
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerTenantId))
        {
            if (Guid.TryParse(headerTenantId.FirstOrDefault(), out var parsedHeaderId))
            {
                tenantId = parsedHeaderId;
            }
        }

        // Then try JWT claim 'tid'
        if (!tenantId.HasValue && context.User.Identity?.IsAuthenticated == true)
        {
            var claim = context.User.FindFirst("tid") ?? context.User.FindFirst(ClaimTypes.GroupSid);
            if (claim != null && Guid.TryParse(claim.Value, out var parsedClaimId))
            {
                tenantId = parsedClaimId;
            }
        }

        if (tenantId.HasValue)
        {
            tenantService.SetCurrentTenantId(tenantId.Value);
            context.Items["TenantId"] = tenantId.Value;
        }
        else
        {
            _logger.LogWarning("No tenant ID resolved for request {Path}", context.Request.Path);
        }

        await _next(context).ConfigureAwait(false);
    }
}
