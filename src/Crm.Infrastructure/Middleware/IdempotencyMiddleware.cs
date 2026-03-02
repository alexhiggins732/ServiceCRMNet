using System.Text;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, CrmDbContext dbContext)
    {
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var keyString = idempotencyKey.ToString();
        var keyRecord = await dbContext.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == keyString).ConfigureAwait(false);

        if (keyRecord != null)
        {
            if (keyRecord.ExpiresAt < DateTime.UtcNow)
            {
                // Key expired, process as normal
                _logger.LogInformation("Expired idempotency key {Key} received, processing as new request.", keyString);
            }
            else
            {
                _logger.LogInformation("Returning cached response for idempotency key {Key}.", keyString);
                context.Response.StatusCode = keyRecord.StatusCode;
                if (!string.IsNullOrEmpty(keyRecord.ResponseBody))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(keyRecord.ResponseBody).ConfigureAwait(false);
                }
                return;
            }
        }

        // Intercept response stream to capture output
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context).ConfigureAwait(false);

        // Save new key
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync().ConfigureAwait(false);
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var newKeyRecord = new IdempotencyKey
        {
            Key = keyString,
            RequestPath = context.Request.Path,
            StatusCode = context.Response.StatusCode,
            ResponseBody = responseText,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // configurable expiry
        };

        dbContext.IdempotencyKeys.Add(newKeyRecord);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        await responseBody.CopyToAsync(originalBodyStream).ConfigureAwait(false);
    }
}
