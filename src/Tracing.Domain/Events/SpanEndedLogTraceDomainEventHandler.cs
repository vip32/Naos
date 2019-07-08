namespace Naos.Core.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SpanEndedLogTraceDomainEventHandler
        : IDomainEventHandler<SpanStartedDomainEvent>, IDomainEventHandler<SpanEndedDomainEvent>
    {
        private readonly ILogger<SpanEndedLogTraceDomainEventHandler> logger;

        public SpanEndedLogTraceDomainEventHandler(ILogger<SpanEndedLogTraceDomainEventHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(SpanEndedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public bool CanHandle(SpanStartedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public Task Handle(SpanEndedDomainEvent notification, CancellationToken cancellationToken)
        {
            if(notification?.Span != null)
            {
                using(this.logger.BeginScope(new Dictionary<string, object>()
                {
                    // map Span to LogTrace properties
                    [LogPropertyKeys.TrackType] = LogTrackTypes.Trace,
                    [LogPropertyKeys.TrackName] = notification.Span.OperationName,
                    [LogPropertyKeys.TrackId] = notification.Span.SpanId,
                    [LogPropertyKeys.TrackParentId] = notification.Span.ParentSpanId,
                    [LogPropertyKeys.TrackKind] = notification.Span.Kind?.ToString(),
                    [LogPropertyKeys.TrackStatus] = notification.Span.Status?.ToString(),
                    [LogPropertyKeys.TrackStatusDescription] = notification.Span.StatusDescription,
                    [LogPropertyKeys.TrackStartTime] = notification.Span.StartTime?.ToString("o"),
                    [LogPropertyKeys.TrackEndTime] = notification.Span.EndTime?.ToString("o")
                    // TODO: map all TAGS/LOGS/BAG
                }))
                {
                    try
                    {
                        this.logger.Log(LogLevel.Information, $"{{LogKey:l}} {notification.Span.OperationName:l} (parent={notification.Span.ParentSpanId})", notification.Span.LogKey);
                    }
                    catch(AggregateException ex) // IndexOutOfRangeException
                    {
                        if(ex.InnerException is IndexOutOfRangeException)
                        {
                            this.logger.Log(LogLevel.Warning, $"{{LogKey:l}} {notification.Span.OperationName:l}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task Handle(SpanStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            //this.logger.LogInformation($"{{LogKey:l}} span started: {notification.Span.OperationName} (id={notification.Span.SpanId}, kind={notification.Span.Kind}, tags={string.Join("|", notification.Span.Tags.Select(t => $"{t.Key}={t.Value}"))})", LogKeys.Tracing);
            return Task.CompletedTask;
        }
    }
}
