namespace Naos.ServiceDiscovery.App
{
    public class HealthCheckStrategy
    {
        private readonly IServiceRegistry registry;

        public HealthCheckStrategy(IServiceRegistry registry)
        {
            EnsureThat.EnsureArg.IsNotNull(registry, nameof(registry));

            this.registry = registry;
        }
    }
}
