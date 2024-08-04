using System.Security.Claims;
using System.Text.Json;
using Titeenipeli.Models;
using Titeenipeli.Options;

namespace Titeenipeli.Middleware;

public class JwtDeserializerMiddleware
{
    private readonly RequestDelegate _next;

    public JwtDeserializerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, JwtOptions jwtOptions)
    {
        ClaimsIdentity? identity = (ClaimsIdentity?)httpContext.User.Identity;

        if (identity == null)
        {
            await _next(httpContext);
            return;
        }

        string? json = identity.FindFirst(jwtOptions.ClaimName)?.Value;
        httpContext.Items.Add(jwtOptions.ClaimName, json != null ? JsonSerializer.Deserialize<JwtClaim>(json) : null);

        await _next(httpContext);
    }
}