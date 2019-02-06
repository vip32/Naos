namespace Naos.Core.Messaging.Infrastructure.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Messaging.Domain.Model;
    using Newtonsoft.Json;

    public class FileSystemMessageBroker : IMessageBroker
    {
        private readonly ILogger<FileSystemMessageBroker> logger;
        private readonly IMediator mediator;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly IFileStorage fileStorage;
        private readonly FileSystemConfiguration configuration;
        private readonly ISubscriptionMap map;
        private readonly string filterScope;
        private readonly string messageScope;
        private readonly IDictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        public FileSystemMessageBroker(
            ILogger<FileSystemMessageBroker> logger,
            IMediator mediator,
            IMessageHandlerFactory handlerFactory,
            IFileStorage fileStorage,
            FileSystemConfiguration configuration = null,
            ISubscriptionMap map = null,
            string filterScope = null,
            string messageScope = "local") // message origin service name
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.logger = logger;
            this.mediator = mediator;
            this.handlerFactory = handlerFactory;
            this.fileStorage = fileStorage;
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
                message.CorrelationId = RandomGenerator.GenerateString(13, true); //Guid.NewGuid().ToString().Replace("-", string.Empty);
            }

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogEventKeys.Messaging);
                }

                if (message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.messageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogEventKeys.Messaging);
                }

                // TODO: async publish!
                /*await */
                this.mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/

                var messageName = /*message.Name*/ message.GetType().PrettyName(false);

                // store message in specific (Message) folder
                var directory = this.GetDirectory(messageName, this.filterScope);
                this.EnsureDirectory(directory);

                this.logger.LogInformation("{LogKey:l} publish (name={MessageName}, id={MessageId}, origin={MessageOrigin})", LogEventKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                var fullFileName = Path.Combine(this.GetDirectory(messageName, this.filterScope), $"message_{message.Id}_{this.messageScope}.json.tmp");
                using (var streamWriter = File.CreateText(fullFileName))
                {
                    streamWriter.Write(SerializationHelper.JsonSerialize(message));
                    streamWriter.Flush();
                    //streamWriter.Close();
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
                    this.logger.LogInformation("{LogKey:l} subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, watch={Directory})", LogEventKeys.Messaging, typeof(TMessage).PrettyName(), this.messageScope, this.filterScope, typeof(THandler).Name, directory);
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
                    this.logger.LogDebug("{LogKey:l} filesystem onrenamed handler registered (name={messageName})", LogEventKeys.Messaging, typeof(TMessage).PrettyName());

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
            throw new NotImplementedException();
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
            var messageBody = this.ReadFile(fullPath);

            if (this.map.Exists(messageName))
            {
                foreach (var subscription in this.map.GetAll(messageName))
                {
                    var messageType = this.map.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(messageBody, messageType);
                    var message = jsonMessage as Message;
                    if (message?.Origin.IsNullOrEmpty() == true)
                    {
                        //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                        message.Origin = fullPath.SubstringFromLast("_").SubstringTillLast("."); // read metadata from filename
                    }

                    this.logger.LogInformation("{LogKey:l} process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, messageType.PrettyName(), message?.Id, this.messageScope, message.Origin);

                    // construct the handler by using the DI container
                    var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if (handler != null && method != null)
                    {
                        // TODO: async publish!
                        await this.mediator.Publish(new MessageHandledDomainEvent(message, this.messageScope)).AnyContext();
                        await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                    }
                    else
                    {
                        this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, messageType.PrettyName(), this.messageScope, message?.Id, message.Origin);
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogEventKeys.Messaging);
            }

            return processed;
        }

        private string GetDirectory(string messageName, string filterScope)
        {
            return $@"{this.configuration.Folder.EmptyToNull() ?? Path.GetTempPath()}naos_messaging\{filterScope}\{messageName}\".Replace("\\", @"\");
        }

        private void EnsureDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private string ReadFile(string fullPath)
        {
            System.Threading.Thread.Sleep(this.configuration.ProcessDelay); // this helps with locked files
            using (var reader = new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
