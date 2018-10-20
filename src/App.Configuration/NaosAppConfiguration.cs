namespace Naos.Core.App.Configuration
{
    using Microsoft.Extensions.Configuration;

    public class NaosAppConfiguration
    {
        public virtual string Name { get; set; }

        public virtual bool Enabled { get; set; } = true;

        public virtual void Bind(string section)
        {
            NaosConfigurationFactory
                .CreateRoot()
                .GetSection(section)
                .Bind(this);
        }
    }
}