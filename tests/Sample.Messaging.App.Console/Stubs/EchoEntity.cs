namespace Naos.Core.Sample.Messaging.App.Console
{
    using Naos.Core.Domain;

    public class EchoEntity : Entity<string>
    {
        public string Text { get; set; }
    }
}
