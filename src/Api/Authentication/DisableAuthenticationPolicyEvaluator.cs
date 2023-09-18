namespace SearchService.Api.Authentication;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

public class DisableAuthenticationPolicyEvaluator : IPolicyEvaluator
{
    //Always pass authentication.
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var authenticationTicket = new AuthenticationTicket(
            new ClaimsPrincipal(),
            new AuthenticationProperties(),
            JwtBearerDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }

    // Always pass authorization
    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource) => Task.FromResult(PolicyAuthorizationResult.Success());
}
