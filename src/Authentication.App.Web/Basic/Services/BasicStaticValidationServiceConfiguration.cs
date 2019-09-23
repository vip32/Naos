namespace Naos.Authentication.App.Web
{
    using System.Collections.Generic;

    public class BasicStaticValidationServiceConfiguration
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public IDictionary<string, string> Claims { get; set; }
    }
}