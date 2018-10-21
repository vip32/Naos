namespace Naos.Core.Messaging
{
    using Domain.Model;

    public class TestMessage : Message
    {
        public string Data { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class DummyMessage : Message
#pragma warning restore SA1402 // File may only contain a single class
    {
        public string Data { get; set; }
    }
}