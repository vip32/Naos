namespace Naos.Sample.IntegrationTests.Customers.App
{
    using System.Threading.Tasks;
    using Bogus;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Sample.Customers.App;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class CreateCustomerCommandHandlerTests : BaseTest
    {
        private readonly IMediator mediator;
        private readonly Faker<Customer> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public CreateCustomerCommandHandlerTests()
        {
            this.mediator = this.ServiceProvider.GetService<IMediator>();
            this.entityFaker = new Faker<Customer>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId);
        }

        [Fact]
        public async Task Command_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            var command = new CreateCustomerCommand(entity);

            // act
            var response = await this.mediator.Send(command).AnyContext();

            // assert
            response.ShouldNotBeNull();
            response.Result.ShouldNotBeNull();
            response.Cancelled.ShouldBeFalse();
            response.CancelledReason.ShouldBeNullOrEmpty();
            command.Customer.ShouldNotBeNull();
            command.Customer.Id.ShouldNotBeNullOrEmpty();
            command.Customer.CustomerNumber.ShouldNotBeNullOrEmpty();
            command.Properties.ShouldContainKey(typeof(CreateCustomerCommandHandler).Name);
        }

        [Fact]
        public async Task InvalidCommandThrowsValidationException_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            entity.FirstName = null; // causes validator to fail (ValidateCommandBehavior)
            var command = new CreateCustomerCommand(entity);

            // act/assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => this.mediator.Send(command)).AnyContext();
        }

        //[Fact]
        //public async Task InvalidCommandIsCancelled_Test()
        //{
        //    // arrange
        //    var entity = this.entityFaker.Generate();
        //    entity.FirstName = null; // causes validator to fail (ValidateCommandBehavior)
        //    var command = new CreateCustomerCommand(entity);

        //    // act
        //    var response = await this.mediator.Send(command).AnyContext();

        //    // assert
        //    response.ShouldNotBeNull();
        //    response.Result.ShouldBeNull();
        //    response.Cancelled.ShouldBeTrue();
        //    response.CancelledReason.ShouldNotBeNullOrEmpty();
        //}

        [Fact]
        public async Task CancelledCommand_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            entity.Region = "South"; // handler will cancels customer creation in this region
            var command = new CreateCustomerCommand(entity);

            // act
            var response = await this.mediator.Send(command).AnyContext();

            // assert
            response.ShouldNotBeNull();
            response.Result.ShouldBeNull();
            response.Cancelled.ShouldBeTrue();
            response.CancelledReason.ShouldNotBeNullOrEmpty();
            command.Customer.ShouldNotBeNull();
            command.Customer.Id.ShouldBeNullOrEmpty();
            command.Customer.CustomerNumber.ShouldBeNullOrEmpty();
            command.Properties.ShouldContainKey(typeof(CreateCustomerCommandHandler).Name);
        }
    }
}
