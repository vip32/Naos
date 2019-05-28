namespace Naos.Core.Messaging.Domain
{
    using System.Threading.Tasks;

    public interface IMessageHandler<T>
        where T : Message
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        Task Handle(T message);
    }
}