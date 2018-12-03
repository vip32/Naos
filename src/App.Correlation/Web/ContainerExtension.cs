namespace Naos.Core.App.Correlation.Web
{
    using SimpleInjector;

    /// <summary>
    /// Extensions on the <see cref="Container"/>.
    /// </summary>
    public static class ContainerExtension
    {
        /// <summary>
        /// Adds required services to support the Correlation ID functionality.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container AddNaosCorrelation(this Container container)
        {
            container.Register<ICorrelationContextAccessor, CorrelationContextAccessor>(Lifestyle.Singleton);
            container.Register<ICorrelationContextFactory, CorrelationContextFactory>(Lifestyle.Transient);

            return container;
        }
    }
}
