namespace Naos.Core.Commands.Domain
{
    public class PingCommand : CommandRequest<object> // has no response type
    {
        public string MyProperty { get; set; }

        public int MyIntProperty { get; set; }
        // public Abc Data { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class Abc
#pragma warning restore SA1402 // File may only contain a single type
    {
        public int MyProperty1 { get; set; }

        public string MyProperty2 { get; set; }
    }
}
