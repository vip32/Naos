namespace Naos.Core.Messaging.Infrastructure.Azure.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Naos.Core.Messaging.Domain.Model;

    public class SignalRServerlessMessageBroker : IMessageBroker
    {
        public void Publish(Message message)
        {
            // TODO: publish by http posting to hub endpoint https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ServerHandler.cs
            //       hubname = 'naos_messaging' (TestMessage)
            //       group = messageName
            throw new NotImplementedException();
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            // TODO: subscribe by connecting to the hub and registering SendMessage handler (=ProcessMessage)
            //       hubname = 'naos_messaging' (TestMessage)
            //       group = messageName
            //       https://github.com/aspnet/AzureSignalR-samples/blob/master/samples/Serverless/ClientHandler.cs

            //var _connection = new HubConnectionBuilder()
            //    .WithUrl(url, option =>
            //    {
            //        option.AccessTokenProvider = () =>
            //        {
            //            return Task.FromResult(serviceUtils.GenerateAccessToken(url, userId));
            //        };
            //    }).Build();

            //_connection.On<string, string>(
            //    "SendMessage",
            //    (string server, string message) =>
            //    {
            //        Console.WriteLine($"[{DateTime.Now.ToString()}] Received message from server {server}: {message}");
            //    });

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
