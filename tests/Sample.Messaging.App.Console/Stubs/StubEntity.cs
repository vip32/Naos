namespace Naos.Core.Sample.Messaging.App.Console
{
    using Naos.Core.Domain;

    public class StubEntity : Entity<string>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
