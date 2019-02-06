namespace Naos.Core.Common.Serialization
{
    public static class DefaultSerializer
    {
        public static ISerializer Instance { get; set; } = new MessagePackSerializer(); // TODO: rename to create
    }
}
