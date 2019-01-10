namespace Naos.Core.Messaging.Infrastructure.Azure.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Naos.Core.Messaging.Domain.Model;

    public class SignalRServerlessMessageBroker : IMessageBroker
    {
        public void Publish(Message message)
        {
            // TODO: publish by http posting to hub endpoint https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ServerHandler.cs
            throw new NotImplementedException();
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            // TODO: subscribe by connecting to the hub and registering SendMessage handler (=ProcessMessage)
            //       https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ClientHandler.cs
            throw new NotImplementedException();
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            throw new NotImplementedException();
        }
    }
}
