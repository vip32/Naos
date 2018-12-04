namespace Naos.Core.Common.Dependency.SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using global::SimpleInjector;
    using MediatR;
    using MediatR.Pipeline;

    public static class ContainerExtensions
    {
        public static Container AddNaosMediator(this Container container, params Assembly[] assemblies)
        {
            return BuildMediator(container, (IEnumerable<Assembly>)assemblies);
        }

        public static Container BuildMediator(this Container container, IEnumerable<Assembly> assemblies)
        {
            // https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.SimpleInjector/Program.cs

            var allAssemblies = new List<Assembly> { typeof(IMediator).GetTypeInfo().Assembly };
            allAssemblies.AddRange(assemblies);

            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(typeof(IRequestHandler<,>), allAssemblies);

            // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
            var notificationHandlerTypes = container.GetTypesToRegister(typeof(INotificationHandler<>), allAssemblies, new TypesToRegisterOptions
            {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });
            container.Collection.Register(typeof(INotificationHandler<>), notificationHandlerTypes);

            //Pipeline
            container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
                //typeof(GenericPipelineBehavior<,>)
            });
            container.Collection.Register(typeof(IRequestPreProcessor<>), Enumerable.Empty<Type>());
            container.Collection.Register(typeof(IRequestPostProcessor<,>), Enumerable.Empty<Type>());

            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);

            //container.Verify();

            return container;
        }
    }
}