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
        private readonly string stream;

        public EventStoreTest()
        {
            this.connection = EventStoreConnection.Create(new Uri("tcp://localhost:1113"));
            this.connection.ConnectAsync().Wait();
            this.stream = Guid.NewGuid().ToString();
        }

        [Fact]
        public async Task TestStreamDoesNotExists()
        {
            var events = await this.connection.ReadStreamEventsForwardAsync(this.stream, StreamPosition.Start, 1, false).AnyContext();

            Assert.Equal(SliceReadStatus.StreamNotFound, events.Status);
        }

        [Fact]
        public async Task TestStreamExists()
        {
            await this.AppendEventToStreamAsync().AnyContext();

            var events = await this.connection.ReadStreamEventsForwardAsync(this.stream, StreamPosition.Start, 1, false).AnyContext();

            Assert.Equal(SliceReadStatus.Success, events.Status);
            Assert.Single(events.Events);
        }

        [Fact]
        public async Task TestPerformance()
        {
            for (var i = 0; i < 100; i++)
            {
                await this.connection.AppendToStreamAsync(this.stream, i - 1,
                    new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{}"), StreamMetadata.Create().AsJsonBytes())).AnyContext();
            }
        }

        public void Dispose()
        {
            this.connection.DeleteStreamAsync(this.stream, ExpectedVersion.Any).Wait();
            this.connection.Dispose();
        }

        private async Task AppendEventToStreamAsync()
        {
            await this.connection.AppendToStreamAsync(this.stream, ExpectedVersion.NoStream,
                new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{}"), StreamMetadata.Create().AsJsonBytes())).AnyContext();
        }
    }
}