namespace Naos.Core.Common.Serialization
{
    public static class DefaultSerializer
    {
        public static ISerializer Create { get; set; } = new MessagePackSerializer(); // TODO: as method()
    }
}
