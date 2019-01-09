namespace Naos.Core.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Domain.Model;

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
        /// <typeparam name="TM">The type of the message.</typeparam>
        /// <typeparam name="TH">The type of the message handler.</typeparam>
        public void Add<TM, TH>()
            where TM : Message
            where TH : IMessageHandler<TM>
        {
            this.Add(this.GetKey<TM>(), typeof(TH));
            this.messageTypes.Add(typeof(TM));
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <typeparam name="TH">The type of the handler.</typeparam>
        public void Remove<TM, TH>()
            where TM : Message
            where TH : IMessageHandler<TM> => this.Remove(this.GetKey<TM>(), this.Find<TM, TH>());

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, IEnumerable<SubscriptionDetails>> GetAll()
        {
            var result = new Dictionary<string, IEnumerable<SubscriptionDetails>>();
            foreach(var i in this.map.NullToEmpty())
            {
                result.Add(i.Key, i.Value.AsEnumerable());
            }

            return result;
        }

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        public IEnumerable<SubscriptionDetails> GetAll<TM>()
            where TM : Message => this.GetAll(this.GetKey<TM>());

        /// <summary>
        /// Gets a specific subscription detail.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        public IEnumerable<SubscriptionDetails> GetAll(string messageName) => this.map[messageName].NullToEmpty();

        /// <summary>
        /// Does this instance exist in the map.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        public bool Exists<TM>()
            where TM : Message => this.Exists(this.GetKey<TM>());

        /// <summary>
        /// Does the specified message name exist in the map.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        public bool Exists(string messageName) => this.map.ContainsKey(messageName); // TODO: achtung casing!

        /// <summary>
        /// Gets the message type by name.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        public Type GetByName(string messageName) =>
            //this.messageTypes.SingleOrDefault(t => t.Name == messageName);
            this.messageTypes.SingleOrDefault(t => this.GetKey(t).Equals(messageName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        //public string GetKey<TMessage>() => typeof(TMessage).Name;
        public string GetKey<TM>() => typeof(TM).PrettyName();

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

        private SubscriptionDetails Find<TM, TH>()
             where TM : Message
             where TH : IMessageHandler<TM> => this.FindSubscription(this.GetKey<TM>(), typeof(TH));

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
