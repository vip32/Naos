namespace Naos.Core.Messaging
{
    using System;
    using System.Collections.Generic;
    using Domain.Model;

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
        /// <typeparam name="TM"></typeparam>
        /// <typeparam name="TH">The type of the h.</typeparam>
        void Add<TM, TH>()
           where TM : Message
           where TH : IMessageHandler<TM>;

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <typeparam name="TH">The type of the h.</typeparam>
        /// <param name="messageName"></param>
        void Add<TM, TH>(string messageName)
           where TM : Message
           where TH : IMessageHandler<TM>;

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <typeparam name="TH">The type of the h.</typeparam>
        void Remove<TM, TH>()
             where TM : Message
             where TH : IMessageHandler<TM>;

        /// <summary>
        /// Does this instance exist in the map.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        bool Exists<TM>()
            where TM : Message;

        /// <summary>
        /// Does the specified message name exist in the map.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        bool Exists(string messageName);

        /// <summary>
        /// Gets the message type by name.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        Type GetByName(string messageName);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, IEnumerable<SubscriptionDetails>> GetAll();

        /// <summary>
        /// Gets all subscription details.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        IEnumerable<SubscriptionDetails> GetAll<TM>()
            where TM : Message;

        /// <summary>
        /// Gets specific subscription details.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <returns></returns>
        IEnumerable<SubscriptionDetails> GetAll(string messageName);

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        string GetKey<TM>();
    }
}
