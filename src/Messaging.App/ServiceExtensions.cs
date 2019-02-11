namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddMessaging(this ServiceConfigurationContext context)
        {
            return context;
        }
    }
}