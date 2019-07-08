namespace Naos.Core.Operations.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Foundation.Infrastructure.Azure;

    public class LogAnalyticsLogEventRepository : LogAnalyticsRepository<LogEvent>, ILogEventRepository
    {
        public LogAnalyticsLogEventRepository(
            ILoggerFactory loggerFactory,
            HttpClient httpClient,
            LogAnalyticsConfiguration configuration,
            string accessToken,
            IEnumerable<LogAnalyticstEntityMap> entityMap = null)
            : base(loggerFactory, httpClient, configuration, accessToken, entityMap)
        {
        }
    }
}
