namespace Naos.Core.Authentication.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Policy;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Naos.Foundation;

    public class EasyAuthHeadersRequirementHandler : AuthorizationHandler<EasyAuthHeadersRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EasyAuthHeadersRequirement requirement)
        {
            if(!context.User.Identity.IsAuthenticated && !requirement.Provider.IsNullOrEmpty())
            {
                if(context.Resource is AuthorizationFilterContext redirectContext)
                {
                    //redirectContext.HttpContext.Response.Redirect($"/.auth/login/{requirement.Provider}");
                    redirectContext.Result = new RedirectResult($"/.auth/login/{requirement.Provider}");
                }
            }

            context.Succeed(requirement); // needed
            return Task.CompletedTask;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class RedirectingPolicyEvaluator : PolicyEvaluator
#pragma warning restore SA1402 // File may only contain a single type
    {
        public RedirectingPolicyEvaluator(IAuthorizationService authorization)
            : base(authorization)
        {
        }

        public override async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object resource)
        {
            var result = await base.AuthorizeAsync(policy, authenticationResult, context, resource);
            if(!result.Succeeded)
            {
                // If user is not authenticated, send them to the easyauth login
                if(!context.User.Identity.IsAuthenticated)
                {
                    // Redirect the user
                    context.Response.Redirect($"/.auth/login/aad");

                    // Return success since we've handled it now
                    return PolicyAuthorizationResult.Success();
                }
            }

            return result;
        }
    }
}
