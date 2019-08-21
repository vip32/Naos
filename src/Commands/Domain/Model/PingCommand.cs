namespace Naos.Core.Commands.Domain
{
    public class PingCommand : CommandRequest<object> // has no response type
    {
        public string MyProperty { get; set; }
    }
}
