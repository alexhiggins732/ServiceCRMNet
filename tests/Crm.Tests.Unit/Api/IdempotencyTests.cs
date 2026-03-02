using Crm.Infrastructure.Middleware;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Crm.Domain.Entities;

namespace Crm.Tests.Unit.Api;

public class IdempotencyTests
{
    [Fact]
    public async Task InvokeAsync_WithExistingKey_ReturnsCachedResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: "IdempotencyDb_" + Guid.NewGuid())
            .Options;

        var loggerMock = new Mock<ILogger<IdempotencyMiddleware>>();

        using (var context = new CrmDbContext(options, Mock.Of<Crm.Application.Common.Interfaces.ITenantService>()))
        {
            context.IdempotencyKeys.Add(new IdempotencyKey
            {
                Key = "test-key",
                RequestPath = "/api/test",
                StatusCode = 201,
                ResponseBody = "{\"status\":\"cached\"}",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
            await context.SaveChangesAsync();
        }

        using var testContext = new CrmDbContext(options, Mock.Of<Crm.Application.Common.Interfaces.ITenantService>());
        var middleware = new IdempotencyMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: loggerMock.Object
        );

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Headers["Idempotency-Key"] = "test-key";
        httpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(httpContext, testContext);

        // Assert
        Assert.Equal(201, httpContext.Response.StatusCode);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        Assert.Equal("{\"status\":\"cached\"}", responseBody);
    }
}
