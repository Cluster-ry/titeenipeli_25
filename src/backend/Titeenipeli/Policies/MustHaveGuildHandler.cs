using Microsoft.AspNetCore.Authorization;
using Titeenipeli.Options;

namespace Titeenipeli.Policies;

public class MustHaveGuildHandler : AuthorizationHandler<MustHaveGuildRequirement>
{
    private readonly JwtOptions _jwtOptions;

    public MustHaveGuildHandler(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   MustHaveGuildRequirement requirement)
    {
        if (context.User.HasClaim(claim =>
                claim.Type == _jwtOptions.GuildClaimName && claim.Issuer == _jwtOptions.ValidIssuer))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}