namespace Naos.Core.Configuration.App
{
    using Microsoft.Extensions.Configuration;

    public class NaosAppConfiguration
    {
        public virtual string Name { get; set; }

        public virtual bool Enabled { get; set; } = true;

        public virtual void Bind(string section)
        {
            NaosConfigurationFactory
                .Create()
                .GetSection(section)
                .Bind(this);
        }
    }
}