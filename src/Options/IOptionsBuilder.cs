namespace Naos.Foundation
{
    public interface IOptionsBuilder
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object Target { get; }
    }

    public interface IOptionsBuilder<out T> : IOptionsBuilder
    {
        T Build();
    }
}
