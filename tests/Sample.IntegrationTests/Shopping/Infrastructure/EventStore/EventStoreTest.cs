namespace Naos.Sample.IntegrationTests.Shopping.Infrastructure
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using Naos.Foundation;
    using Xunit;

    public class EventStoreTest : IDisposable
    {
        private readonly IEventStoreConnection connection;
        private readonly string streamName;

        public EventStoreTest()
        {
            this.connection = EventStoreConnection.Create(new Uri("tcp://localhost:1113"), "naos.test");
            this.connection.ConnectAsync().Wait();
            this.streamName = $"TestAggregate-{Guid.NewGuid()}";
        }

        [Fact]
        public async Task TestStreamDoesNotExists()
        {
            var events = await this.connection.ReadStreamEventsForwardAsync(this.streamName, StreamPosition.Start, 1, false).AnyContext();

            Assert.Equal(SliceReadStatus.StreamNotFound, events.Status);
        }

        [Fact]
        public async Task TestStreamExists()
        {
            await this.AppendEventToStreamAsync().AnyContext();

            var events = await this.connection.ReadStreamEventsForwardAsync(this.streamName, StreamPosition.Start, 1, false).AnyContext();

            Assert.Equal(SliceReadStatus.Success, events.Status);
            Assert.Single(events.Events);
        }

        [Fact]
        public async Task TestPerformance()
        {
            for (var i = 0; i < 1000; i++)
            {
                await this.connection.AppendToStreamAsync(this.streamName, i - 1,
                    new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{}"), StreamMetadata.Create().AsJsonBytes())).AnyContext();
            }
        }

        public void Dispose()
        {
            this.connection.DeleteStreamAsync(this.streamName, ExpectedVersion.Any).Wait();
            this.connection.Dispose();
        }

        private async Task AppendEventToStreamAsync()
        {
            await this.connection.AppendToStreamAsync(this.streamName, ExpectedVersion.NoStream,
                new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{}"), StreamMetadata.Create().AsJsonBytes())).AnyContext();
        }
    }
}