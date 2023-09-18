namespace SearchService.Api;

using System.Security.Claims;
using Constants;
using Microsoft.AspNetCore.Authorization;

internal static class AuthorizationExtensions
{
    /// <summary>
    ///     Adds custom authentication.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns></returns>
    public static IServiceCollection AddAuthZ(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var requireAvailiaAccessPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(PermitApiAccess())
                .Build();

            options.FallbackPolicy = requireAvailiaAccessPolicy;

            options.AddPolicy(PolicyConstants.CoordinatorAssistantAccess, requireAvailiaAccessPolicy);

            options.AddPolicy(PolicyConstants.AdminPolicy, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role, PolicyConstants.AdminRole));
        });

        return services;

        static Func<AuthorizationHandlerContext, bool> PermitApiAccess()
        {
            return context =>
                context.User.HasClaim(PolicyConstants.CoordinatorAssistantAccessClaim, "true")
                || context.User.HasClaim(PolicyConstants.ServiceAccountAccessClaim, "true");
        }
    }
}
