using Microsoft.Extensions.Primitives;

namespace Motix.Security;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-API-KEY";

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, IConfiguration config)
    {
        var path = ctx.Request.Path.Value ?? "";
        var method = ctx.Request.Method.ToUpperInvariant();

        // libera swagger e health
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        // exige chave só para escrita na API versionada (POST/PUT/DELETE/PATCH)
        var isVersionedApi = path.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase);
        var isWrite = method is "POST" or "PUT" or "DELETE" or "PATCH";

        if (isVersionedApi && isWrite)
        {
            if (!ctx.Request.Headers.TryGetValue(HeaderName, out StringValues key))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("API Key missing. Use header X-API-KEY.");
                return;
            }

            var expected = config.GetValue<string>("ApiKey");
            if (string.IsNullOrEmpty(expected) || key != expected)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("Invalid API Key.");
                return;
            }
        }

        await _next(ctx);
    }
}