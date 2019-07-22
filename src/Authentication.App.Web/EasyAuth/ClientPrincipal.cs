namespace Naos.Core.Authentication.App.Web
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ClientPrincipal
    {
        [JsonProperty("auth_typ")]
        public string AuthenticationType { get; set; }

        [JsonProperty("claims")]
        public IEnumerable<UserClaim> Claims { get; set; }

        [JsonProperty("name_typ")]
        public string NameType { get; set; }

        [JsonProperty("role_typ")]
        public string RoleType { get; set; }
    }
}
