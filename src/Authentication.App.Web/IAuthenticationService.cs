namespace Naos.Authentication.App.Web
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IAuthenticationService
    {
        (bool Authenticated, IEnumerable<Claim> Claims) Validate(string value);
    }
}