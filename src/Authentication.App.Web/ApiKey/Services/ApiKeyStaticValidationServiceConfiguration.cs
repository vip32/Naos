namespace Naos.Core.Authentication.App.Web
{
    using System.Collections.Generic;

    public class ApiKeyStaticValidationServiceConfiguration
    {
        public string ApiKey { get; set; }

        public string UserName { get; set; } // used as request identity (claim)

        public IDictionary<string, string> Claims { get; set; }
    }
}