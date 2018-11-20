namespace Naos.Core.Infrastructure.Azure.Ad
{
    public class AuthenticationConfiguration
    {
        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Resource { get; set; }
    }
}
