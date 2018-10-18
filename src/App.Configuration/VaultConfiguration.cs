namespace Naos.Core.App.Configuration
{
    public partial class NaosConfiguration
    {
        public class VaultConfiguration
        {
            public bool Enabled { get; set; }

            public string Name { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }
        }
    }
}
