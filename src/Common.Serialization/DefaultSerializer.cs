namespace Naos.Core.Common.Serialization
{
    public static class DefaultSerializer // TODO: factory?
    {
        public static ISerializer Create { get; set; } = new MessagePackSerializer(); // TODO: as method()
    }
}
