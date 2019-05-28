namespace Naos.Core.Messaging
{
    using System;
    using System.Collections.Generic;
    using Naos.Core.Messaging.Domain;

    public interface ISubscriptionMap
    {
        /// <summary>
        /// Occurs when [on removed].
        /// </summary>
        event EventHandler<string> OnRemoved;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="THandler">The type of the h.</typeparam>
        void Add<TMessage, THandler>()
           where TMessage : Message
           where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="THandler">The type of the h.</typeparam>
        /// <param name="messageName"></param>
        void Add<TMessage, THandler>(string messageName)
           where TMessage : Message
           where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="THandler">The type of the h.</typeparam>
        void Remove<TMessage, THandler>()
             where TMessage : Message
             where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Does this instance exist in the map.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        bool Exists<TMessage>()
            where TMessage : Message;

        /// <summary>
        /// Does the specified message name exist in the map.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        bool Exists(string messageName);

        /// <summary>
        /// Gets the message type by name.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        Type GetByName(string messageName);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        IReadOnlyDictionary<string, IEnumerable<SubscriptionDetails>> GetAll();

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        IEnumerable<SubscriptionDetails> GetAll<TMessage>()
            where TMessage : Message;

        /// <summary>
        /// Gets specific subscription details.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        IEnumerable<SubscriptionDetails> GetAll(string messageName);

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        string GetKey<TMessage>();
    }
}
