namespace Naos.Core.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Messaging.Domain;
    using Naos.Foundation;

    public class SubscriptionMap : ISubscriptionMap
    {
        private readonly IDictionary<string, List<SubscriptionDetails>> map;
        private readonly IList<Type> messageTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionMap"/> class.
        /// </summary>
        public SubscriptionMap()
        {
            this.map = new Dictionary<string, List<SubscriptionDetails>>();
            this.messageTypes = new List<Type>();
        }

        /// <summary>
        /// Occurs when [on removed].
        /// </summary>
        public event EventHandler<string> OnRemoved;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => this.map.Keys.Count == 0;

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() => this.map.Clear();

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        public void Add<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            this.Add(this.GetKey<TMessage>(), typeof(THandler));
            this.messageTypes.Add(typeof(TMessage));
        }

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        /// <param name="messageName"></param>
        public void Add<TMessage, THandler>(string messageName)
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            this.Add(messageName, typeof(THandler));
            this.messageTypes.Add(typeof(TMessage));
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        public void Remove<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage> => this.Remove(this.GetKey<TMessage>(), this.Find<TMessage, THandler>());

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<SubscriptionDetails>> GetAll()
        {
            var result = new Dictionary<string, IEnumerable<SubscriptionDetails>>();
            foreach (var i in this.map.Safe())
            {
                result.Add(i.Key, i.Value.AsEnumerable());
            }

            return result;
        }

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public IEnumerable<SubscriptionDetails> GetAll<TMessage>()
            where TMessage : Message => this.GetAll(this.GetKey<TMessage>());

        /// <summary>
        /// Gets a specific subscription detail.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public IEnumerable<SubscriptionDetails> GetAll(string messageName) => this.map[messageName].Safe();

        /// <summary>
        /// Does this instance exist in the map.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public bool Exists<TMessage>()
            where TMessage : Message => this.Exists(this.GetKey<TMessage>());

        /// <summary>
        /// Does the specified message name exist in the map.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public bool Exists(string messageName) => this.map.ContainsKey(messageName); // TODO: achtung casing!

        /// <summary>
        /// Gets the message type by name.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public Type GetByName(string messageName) =>
            //this.messageTypes.SingleOrDefault(t => t.Name == messageName);
            this.messageTypes.SingleOrDefault(t => this.GetKey(t).Equals(messageName, StringComparison.OrdinalIgnoreCase)
                || t.PrettyName(false).Equals(messageName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        //public string GetKey<TMessage>() => typeof(TMessage).Name;
        public string GetKey<TMessage>() => typeof(TMessage).PrettyName();

        private string GetKey(Type t) => t.PrettyName();

        private void RaiseOnRemoved(string messageName)
        {
            var handler = this.OnRemoved;
            handler?.Invoke(this, messageName);
        }

        private void Remove(string messageName, SubscriptionDetails subscription)
        {
            if (subscription != null)
            {
                this.map[messageName].Remove(subscription);
                if (!this.map[messageName].Any())
                {
                    this.map.Remove(messageName);
                    var messageType = this.messageTypes.SingleOrDefault(e => e.Name == messageName);
                    if (messageType != null)
                    {
                        this.messageTypes.Remove(messageType);
                    }

                    this.RaiseOnRemoved(messageName);
                }
            }
        }

        private void Add(string messageName, Type handlerType)
        {
            if (!this.Exists(messageName))
            {
                this.map.Add(messageName, new List<SubscriptionDetails>());
            }

            if (this.map[messageName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"handler {handlerType.Name} already registered for '{messageName}'", nameof(handlerType));
            }

            this.map[messageName].Add(SubscriptionDetails.Create(handlerType));
        }

        private SubscriptionDetails Find<TMessage, THandler>()
             where TMessage : Message
             where THandler : IMessageHandler<TMessage> => this.FindSubscription(this.GetKey<TMessage>(), typeof(THandler));

        private SubscriptionDetails FindSubscription(string messageName, Type handlerType)
        {
            if (!this.Exists(messageName))
            {
                return null;
            }

            return this.map[messageName].SingleOrDefault(s => s.HandlerType == handlerType);
        }
    }
}
