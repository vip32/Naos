namespace Naos.Core.Messaging.Domain.Model
{
    /// <summary>
    /// The statuses for an <see cref="Message"/>
    /// </summary>
    public enum MessageStatus
    {
        NotPublished = 0,
        Published = 1,
        PublishedFailed = 2
    }
}
