namespace Naos.Core.Messaging
{
    using System.Threading.Tasks;
    using Domain.Model;

    public interface IMessageHandler<in TMessage>
        where TMessage : Message
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        Task Handle(TMessage message);
    }
}