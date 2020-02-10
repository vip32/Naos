//namespace Naos.Sample.Countries.Application
//{
//    using System;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using EnsureThat;
//    using Microsoft.Extensions.Logging;
//    using Naos.Foundation;
//    using Naos.Foundation.Application;
//    using Naos.Queueing.Domain;

//    public class CountriesExportProcessStartupTask : IStartupTask
//    {
//        private readonly ILogger<CountriesExportProcessStartupTask> logger;
//        private readonly IQueue<CountriesExportData> queue;

//        public CountriesExportProcessStartupTask(ILoggerFactory loggerFactory, IQueue<CountriesExportData> queue)
//        {
//            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
//            EnsureArg.IsNotNull(queue, nameof(queue));

//            this.logger = loggerFactory.CreateLogger<CountriesExportProcessStartupTask>();
//            this.queue = queue;
//        }

//        public TimeSpan? Delay { get; set; }

//        public async Task StartAsync(CancellationToken cancellationToken = default)
//        {
//            await this.queue.ProcessItemsAsync(true, CancellationToken.None).AnyContext(); // > mediator handler (QueueEvent<>)
//        }

//        public Task ShutdownAsync(CancellationToken cancellationToken = default)
//        {
//            this.queue.Dispose();
//            return Task.CompletedTask;
//        }
//    }
//}
