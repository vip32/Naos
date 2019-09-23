namespace Naos.Operations.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Infrastructure;
    using Naos.Foundation.Infrastructure.Azure;
    using Naos.Operations.Domain;

    public class LogAnalyticsLogEventRepository : LogAnalyticsRepository<LogEvent>, ILogEventRepository
    {
        public LogAnalyticsLogEventRepository(
            ILoggerFactory loggerFactory,
            HttpClient httpClient,
            LogAnalyticsConfiguration configuration,
            string accessToken,
            IEnumerable<LogAnalyticsEntityMap> entityMap = null)
            : base(loggerFactory, httpClient, configuration, accessToken, entityMap)
        {
        }
    }
}
