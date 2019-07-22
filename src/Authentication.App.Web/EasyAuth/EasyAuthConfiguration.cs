namespace Naos.Core.Authentication.App.Web
{
    using System.Collections.Generic;

    public class EasyAuthConfiguration
    {
        public string Provider { get; set; }

        public IDictionary<string, string> Claims { get; set; }
    }
}