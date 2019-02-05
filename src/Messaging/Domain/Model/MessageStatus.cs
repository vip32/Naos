namespace Naos.Core.Messaging.Domain.Model
{
    using System.ComponentModel;

    /// <summary>
    /// The statuses for an <see cref="Message"/>
    /// </summary>
    public enum MessageStatus
    {
        [Description("not published")]
        NotPublished = 0,

        [Description("published")]
        Published = 1,

        [Description("published failed")]
        PublishedFailed = 2
    }
}
