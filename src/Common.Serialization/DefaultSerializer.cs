namespace Naos.Core.Common
{
    public static class DefaultSerializer // TODO: factory?
    {
        /// <summary>
        /// Gets the default serializer.
        /// </summary>
        /// <value>
        /// The create.
        /// </value>
        public static ISerializer Create { get; } = new MessagePackSerializer(); // TODO: as method()
    }
}
