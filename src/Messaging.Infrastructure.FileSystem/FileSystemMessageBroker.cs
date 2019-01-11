namespace Naos.Core.Messaging.Infrastructure.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain.Model;
    using Newtonsoft.Json;

    public class FileSystemMessageBroker : IMessageBroker
    {
        private readonly ILogger<FileSystemMessageBroker> logger;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly FileSystemConfiguration configuration;
        private readonly ISubscriptionMap map;
        private readonly string filterScope;
        private readonly string messageScope;
        private readonly IDictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        public FileSystemMessageBroker(
            ILogger<FileSystemMessageBroker> logger,
            IMessageHandlerFactory handlerFactory,
            FileSystemConfiguration configuration = null,
            ISubscriptionMap map = null,
            string filterScope = null,
            string messageScope = "local") // message origin identifier
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));

            this.logger = logger;
            this.handlerFactory = handlerFactory;
            this.configuration = configuration ?? new FileSystemConfiguration();
            this.map = map ?? new SubscriptionMap();
            this.filterScope = filterScope;
            this.messageScope = messageScope ?? AppDomain.CurrentDomain.FriendlyName;
        }

        public void Publish(Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if (message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"set messageId (id={message.Id})");
                }

                var messageName = /*message.Name*/ message.GetType().PrettyName(false);

                // store message in specific (Message) folder
                var directory = this.GetDirectory(messageName, this.filterScope);
                this.EnsureDirectory(directory);

                this.logger.LogInformation("MESSAGE publish (name={MessageName}, id={MessageId}, service={Service})", message.GetType().PrettyName(), message.Id, this.messageScope);

                var fullFileName = Path.Combine(this.GetDirectory(messageName, this.filterScope), $"message_{message.Id}_{this.messageScope}.json.tmp");
                using (var streamWriter = File.CreateText(fullFileName))
                {
                    streamWriter.Write(SerializationHelper.JsonSerialize(message));
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                File.Move(fullFileName, fullFileName.SubstringTillLast(".")); // rename file
            }
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName(false);

            if (!this.map.Exists<TMessage>())
            {
                if (!this.watchers.ContainsKey(messageName))
                {
                    var directory = this.GetDirectory(messageName, this.filterScope);
                    this.logger.LogInformation("MESSAGE subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, watch={Directory})", typeof(TMessage).PrettyName(), this.messageScope, this.filterScope, typeof(THandler).Name, directory);
                    this.EnsureDirectory(directory);

                    var watcher = new FileSystemWatcher(directory)
                    {
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                        EnableRaisingEvents = true,
                        Filter = "*.json"
                    };

                    watcher.Renamed += (sender, e) =>
                    {
                        this.ProcessMessage(e.FullPath).GetAwaiter().GetResult(); // TODO: async!
                    };

                    this.watchers.Add(messageName, watcher);

                    this.map.Add<TMessage, THandler>(messageName);
                }
            }

            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            // remove folder watch
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Processes the message by invoking the message handler.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private async Task<bool> ProcessMessage(string fullPath)
        {
            var processed = false;
            var messageName = fullPath.SubstringTillLast(@"\").SubstringFromLast(@"\");
            var messageBody = this.ReadMessage(fullPath);

            if (this.map.Exists(messageName))
            {
                var messageOrigin = fullPath.SubstringFromLast("_").SubstringTillLast("."); // read metadata from filename
                foreach (var subscription in this.map.GetAll(messageName))
                {
                    var messageType = this.map.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(messageBody, messageType);
                    var message = jsonMessage as Domain.Model.Message;
                    if (message != null)
                    {
                        //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                        message.Origin = messageOrigin;
                    }

                    this.logger.LogInformation("MESSAGE process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            messageType.PrettyName(), message?.Id, this.messageScope, messageOrigin);

                    // construct the handler by using the DI container
                    var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if (handler != null && method != null)
                    {
                        await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                    }
                    else
                    {
                        this.logger.LogWarning("process message failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            messageType.PrettyName(), this.messageScope, message?.Id, messageOrigin);
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"unprocessed: {messageName}");
            }

            return processed;
        }

        private string GetDirectory(string messageName, string filterScope)
        {
            return $@"{this.configuration.Folder}naos_messaging\{filterScope}\{messageName}\".Replace("\\", @"\");
        }

        private void EnsureDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private string ReadMessage(string fullPath)
        {
            System.Threading.Thread.Sleep(this.configuration.ProcessDelay); // this helps with locked files
            using (var reader = new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
