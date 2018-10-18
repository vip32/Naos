namespace Naos.Core.App.Configuration
{
    public partial class NaosConfiguration // TODO: should only contain core configuration (messaging, secrets)
    {
        public NaosAppconfiguration App { get; set; } // TODO: move to different configuration type, because app specific

        public NaosSecretsConfiguration Secrets { get; set; }

        public NaosMessagingConfiguration Messaging { get; set; }
    }
}
