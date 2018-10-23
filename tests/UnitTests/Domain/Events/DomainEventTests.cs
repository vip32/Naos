namespace Naos.Core.UnitTests.Domain.Events
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Xunit;

    public class DomainEventTests
    {
        [Fact]
        public async Task SingleDomainEventIsHandledByAllApplicableHandlersTest()
        {
            // arrange
            var serviceProvider = new ServiceCollection()
                .AddMediatR(typeof(StubDomainEvent).Assembly).BuildServiceProvider();
            var sut = serviceProvider.GetService<IMediator>();
            var notification = new StubDomainEvent { Name = "Name1" };
            var notification3 = new StubDomainEvent3<StubEntity>
            {
                Name = "Name1", Entity = new StubEntity { FirstName = "FirstName1", LastName = "LastName1" }
            };

            await sut.Publish(notification).ConfigureAwait(false);
            await sut.Publish(notification3).ConfigureAwait(false);

            Assert.True(notification.Properties.ContainsKey(typeof(FirstStubDomainEventHandler).Name));
            Assert.True(notification.Properties.ContainsKey(typeof(SecondStubDomainEventHandler).Name));
            Assert.True(notification3.Properties.ContainsKey(typeof(ThirdStubDomainEventHandler).Name));
        }

        [Fact]
        public async Task SingleDomainEventIsHandledByAllApplicableHandlersTest2()
        {
            // arrange
            var serviceProvider = new ServiceCollection()
                //.AddScoped<INotificationHandler<EntityCreateDomainEvent2<IEntity>>, EntityCreateDomainEventDomainEvent2Handler>()
                .AddMediatR(typeof(IEntity).Assembly).BuildServiceProvider();
            var sut = serviceProvider.GetService<IMediator>();
            var notification = new EntityCreateDomainEvent2<StubEntity>(new StubEntity { FirstName = "FirstName1", LastName = "LastName1" });

            await sut.Publish(notification).ConfigureAwait(false);

            //Assert.True(notification.Properties.ContainsKey(typeof(EntityCreateDomainEventDomainEventHandler<StubEntity>).Name));
        }

        public class StubDomainEvent : IDomainEvent
        {
            public string Name { get; set; }

            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        public class StubDomainEvent3<T> : IDomainEvent
        {
            public string Name { get; set; }

            public T Entity { get; set; }

            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        public class StubEntity : Entity<string>, IEntity
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class FirstStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }

        public class SecondStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }

        public class ThirdStubDomainEventHandler : IDomainEventHandler<StubDomainEvent3<StubEntity>>
        {
            public Task Handle(StubDomainEvent3<StubEntity> notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }
    }
}
