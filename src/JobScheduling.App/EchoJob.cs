namespace Naos.Core.JobScheduling.App
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    //using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class EchoJob
    {
        private static readonly Random Random = new Random(DateTime.Now.GetHashCode());
        private readonly ILogger<EchoJob> logger;
        //private readonly ITracer tracer;
        private readonly IHttpClientFactory httpClientFactory;

        public EchoJob(ILogger<EchoJob> logger/*, ITracer tracer*/, IHttpClientFactory httpClientFactory)
        {
            EnsureThat.EnsureArg.IsNotNull(logger, nameof(logger));
            //EnsureThat.EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureThat.EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));

            this.logger = logger;
            //this.tracer = tracer;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task EchoAsync(string text, CancellationToken cancellationToken)
        {
            Thread.Sleep(new TimeSpan(0, 0, 3));

            await Task.Run(() =>
            {
                //using(var scope = this.tracer.BuildSpan(this.GetType().Name.ToLower()).Activate())
                //{
                    if(!text.IsNullOrEmpty())
                    {
                        this.logger.LogInformation($"{{LogKey:l}} {text}", LogKeys.JobScheduling);
                    }

                //}
            }, cancellationToken);
        }

        public Task EchoAsync(string text, CancellationToken cancellationToken, bool breakable)
        {
            Thread.Sleep(new TimeSpan(0, 0, 3));

            if(breakable && Random.Next(2) == 0)// throw randomly
            {
                throw new NaosException("error from job");
            }

            if(!text.IsNullOrEmpty())
            {
                this.logger.LogInformation($"{{LogKey:l}} {text}", LogKeys.JobScheduling);
            }

            return Task.CompletedTask;
        }

        public async Task EchoLongAsync(string text, CancellationToken cancellationToken)
        {
            for(var i = 1; i <= 5; i++)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation($"{{LogKey:l}} job cancelled", LogKeys.JobScheduling);
                    return; //Task.FromCanceled(cancellationToken);
                }

                if(!text.IsNullOrEmpty())
                {
                    this.logger.LogInformation(text);
                }

                // TODO: use typed client here
                var httpClient = this.httpClientFactory.CreateClient("default"); // contains the configured handlers
                var response = await httpClient.PostAsync("http://mockbin.org/request", null, cancellationToken).AnyContext();

                Thread.Sleep(new TimeSpan(0, 0, 45));
                //await Task.Delay(new TimeSpan(0, 0, 45));
            }
        }
    }
}
