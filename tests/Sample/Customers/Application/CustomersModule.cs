namespace Naos.Sample.Customers.Presentation
{
    using Microsoft.Extensions.DependencyInjection;

    public class CustomersModule // =CompositionRoot
    {
        public void Configure(ModuleOptions options, string section = "naos:sample:customers")
        {
            CompositionRoot.AddCustomersModule(options, section);
        }
    }
}
