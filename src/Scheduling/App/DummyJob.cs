namespace Naos.Core.Scheduling.App
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class DummyJob
    {
        private static readonly Random Random = new Random(DateTime.Now.GetHashCode());
        private readonly ILogger<DummyJob> logger;
        private readonly HttpClient httpClient;

        public DummyJob(ILogger<DummyJob> logger, HttpClient httpClient)
        {
            EnsureThat.EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureThat.EnsureArg.IsNotNull(httpClient, nameof(httpClient));

            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task LogMessageAsync(string message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (!message.IsNullOrEmpty())
                {
                    this.logger.LogInformation(message);
                }
            }, cancellationToken);
        }

        public Task LogMessageAsync(string message, CancellationToken cancellationToken, bool breakable)
        {
            if (breakable && Random.Next(2) == 0)// throw randomly
            {
                throw new NaosException("error from job");
            }

            if (!message.IsNullOrEmpty())
            {
                this.logger.LogInformation(message);
            }

            return Task.CompletedTask;
        }

        public async Task LongRunningAsync(string message, CancellationToken cancellationToken)
        {
            for (int i = 1; i <= 5; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation("job cancelled");
                    return; //Task.FromCanceled(cancellationToken);
                }

                var response = await this.httpClient.PostAsync("http://mockbin.org/request", null, cancellationToken).ConfigureAwait(false);

                this.logger.LogInformation(message);
                Thread.Sleep(new TimeSpan(0, 0, 45));
                //await Task.Delay(new TimeSpan(0, 0, 45));
            }
        }
    }
}
