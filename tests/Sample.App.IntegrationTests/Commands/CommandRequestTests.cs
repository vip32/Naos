namespace Naos.Sample.App.IntegrationTests.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Commands.Domain.Model;
    using Naos.Core.Commands.Infrastructure.FileStorage;
    using Naos.Core.Configuration.App;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class CommandRequestTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();
        private readonly IMediator mediator;

        public CommandRequestTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.services
                .AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GetName().Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)).ToArray())
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{RandomGenerator.GenerateString(9)}"))
                    .AddCommands(o => o
                        .AddBehavior<ValidateCommandBehavior>()
                        .AddBehavior<JournalCommandBehavior>()
                        .AddBehavior(new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(f => f
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "commands")))))));

            this.ServiceProvider = this.services.BuildServiceProvider();
            this.mediator = this.ServiceProvider.GetService<IMediator>();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        [Fact]
        public void CanInstantiate_Test()
        {
            this.mediator.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanSend_Test()
        {
            // arrange
            var request = new StubCommandRequest
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // act
            var result = await this.mediator.Send(request).AnyContext();

            // assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanSend_Factory1_Test()
        {
            // arrange
            var request = Factory.Create(typeof(StubCommandRequest));
            request.ShouldNotBeNull();
            //request.SetProperty("FirstName", "John");
            //request.SetProperty("LastName", "Doe");

            // act
            var result = await this.mediator.Send(request).AnyContext();

            // assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanSend_Factory2_Test()
        {
            // arrange
            var request = Activator.CreateInstance(typeof(StubCommandRequest), null); // as ICommandRequest<StubCommandResponse>;
            request.ShouldNotBeNull();
            //request.SetProperty("FirstName", "John");
            //request.SetProperty("LastName", "Doe");

            // act
            var result = await this.mediator.Send(request).AnyContext();

            // assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanSend_Factory3_Test()
        {
            // arrange
            var request = SerializationHelper.JsonDeserialize(
                "{\"FirstName\": \"John\",\"LastName\": \"Doe\"}",
                typeof(StubCommandRequest));
            request.ShouldNotBeNull();

            // act
            var result = await this.mediator.Send(request).AnyContext();

            // assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanSend_Factory4_Test()
        {
            // arrange
            var request = SerializationHelper.JsonDeserialize(
                "{\"Message\": \"Hello World!\"}",
                typeof(EchoCommand));
            request.ShouldNotBeNull();

            // act
            var result = await this.mediator.Send(request).AnyContext();

            // assert
            result.ShouldNotBeNull();
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class StubCommandRequest : BaseCommandRequest<StubCommandResponse>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override ValidationResult Validate() => new Validator().Validate(this);

        private class Validator : AbstractValidator<StubCommandRequest>
        {
            public Validator()
            {
                //this.RuleFor(order => order.FirstName).NotEmpty().WithMessage("FirstName cannot be empty");
                //this.RuleFor(order => order.LastName).NotEmpty().WithMessage("LastName cannot be empty");
            }
        }
    }

    public class StubCommandResponse : BaseCommandResponse
    {
        public string Message { get; set; }
    }

    public class StubCommandHandler : IRequestHandler<StubCommandRequest, StubCommandResponse>
    {
        public async Task<StubCommandResponse> Handle(StubCommandRequest request, CancellationToken cancellationToken)
        {
            return await Task.Run(() => new StubCommandResponse { Message = $"{request.FirstName} {request.LastName}" }).AnyContext();
        }
    }

    //public abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<ICommandRequest<ICommandResponse>, ICommandResponse>
    //    where TRequest : ICommandRequest<TResponse>
    //    where TResponse : ICommandResponse
    //{
    //    public abstract Task<ICommandResponse> Handle(ICommandRequest<ICommandResponse> request, CancellationToken cancellationToken);
    //}
}
