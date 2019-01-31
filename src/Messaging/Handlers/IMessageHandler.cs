namespace Naos.Core.Messaging
{
    using System.Threading.Tasks;
    using Domain.Model;

    public interface IMessageHandler<T>
        where T : Message
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        Task Handle(T message);
    }
}