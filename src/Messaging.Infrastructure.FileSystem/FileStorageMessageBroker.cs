namespace Naos.Core.Messaging.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.Messaging.Domain;
    using Newtonsoft.Json;

    public class FileStorageMessageBroker : IMessageBroker
    {
        private readonly FileStorageMessageBrokerOptions options;
        private readonly ILogger<FileStorageMessageBroker> logger;
        private readonly IDictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        public FileStorageMessageBroker(FileStorageMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));
            EnsureArg.IsNotNull(options.Storage, nameof(options.Storage));

            this.options = options;
            this.options.Folder = options.Folder.EmptyToNull() ?? Path.GetTempPath();
            this.options.Map = options.Map ?? new SubscriptionMap();
            this.options.MessageScope = options.MessageScope ?? AppDomain.CurrentDomain.FriendlyName;
            this.logger = options.CreateLogger<FileStorageMessageBroker>();
        }

        public FileStorageMessageBroker(Builder<FileStorageMessageBrokerOptionsBuilder, FileStorageMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new FileStorageMessageBrokerOptionsBuilder()).Build())
        {
        }

        public void Publish(Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));

            if(message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = RandomGenerator.GenerateString(13, true);
            }

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using(this.logger.BeginScope(loggerState))
            {
                if(message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogEventKeys.Messaging);
                }

                if(message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.options.MessageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogEventKeys.Messaging);
                }

                // store message in specific (Message) folder
                this.logger.LogJournal(LogEventPropertyKeys.TrackPublishMessage, "{LogKey:l} publish (name={MessageName}, id={MessageId}, origin={MessageOrigin})", args: new[] { LogEventKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin });
                var messageName = /*message.Name*/ message.GetType().PrettyName(false);
                var path = Path.Combine(this.GetDirectory(messageName, this.options.FilterScope), $"message_{message.Id}_{this.options.MessageScope}.json.tmp");
                if(this.options.Storage.SaveFileObjectAsync(path, message).Result)
                {
                    this.options.Storage.RenameFileAsync(path, path.SubstringTillLast("."));
                }

                if(this.options.Mediator != null)
                {
                    // TODO: async publish!
                    /*await */
                    this.options.Mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/
                }
            }
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName(false);

            if(!this.options.Map.Exists<TMessage>())
            {
                if(!this.watchers.ContainsKey(messageName))
                {
                    var path = this.GetDirectory(messageName, this.options.FilterScope);
                    this.logger.LogJournal(LogEventPropertyKeys.TrackSubscribeMessage, "{LogKey:l} subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, watch={Directory})", args: new[] { LogEventKeys.Messaging, typeof(TMessage).PrettyName(), this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name, path });
                    this.EnsureDirectory(path);

                    var watcher = new FileSystemWatcher(path)
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

                    this.options.Map.Add<TMessage, THandler>(messageName);
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
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<bool> ProcessMessage(string path)
        {
            var processed = false;
            var messageName = path.SubstringTillLast(@"\").SubstringFromLast(@"\");
            var messageBody = this.GetFileContents(path);

            if(this.options.Map.Exists(messageName))
            {
                foreach(var subscription in this.options.Map.GetAll(messageName))
                {
                    var messageType = this.options.Map.GetByName(messageName);
                    if(messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(messageBody, messageType);
                    var message = jsonMessage as Message;
                    if(message?.Origin.IsNullOrEmpty() == true)
                    {
                        //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                        message.Origin = path.SubstringFromLast("_").SubstringTillLast("."); // read metadata from filename
                    }

                    this.logger.LogJournal(LogEventPropertyKeys.TrackReceiveMessage, "{LogKey:l} process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                        args: new[] { LogEventKeys.Messaging, messageType.PrettyName(), message?.Id, this.options.MessageScope, message.Origin });

                    // construct the handler by using the DI container
                    var handler = this.options.HandlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                    var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                    var method = concreteType.GetMethod("Handle");
                    if(handler != null && method != null)
                    {
                        // TODO: async publish!
                        if(this.options.Mediator != null)
                        {
                            await this.options.Mediator.Publish(new MessageHandledDomainEvent(message, this.options.MessageScope)).AnyContext();
                        }

                        await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                    }
                    else
                    {
                        this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, messageType.PrettyName(), this.options.MessageScope, message?.Id, message.Origin);
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
            return $@"{this.options.Folder.EmptyToNull() ?? Path.GetTempPath()}naos_messaging\{filterScope}\{messageName}\".Replace("\\", @"\");
        }

        private void EnsureDirectory(string fullPath)
        {
            if(!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private string GetFileContents(string path)
        {
            Thread.Sleep(this.options.ProcessDelay); // this helps with locked files
            return this.options.Storage.GetFileContentsAsync(path).Result;
        }
    }
}
