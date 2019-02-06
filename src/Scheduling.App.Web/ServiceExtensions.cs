namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling.App.Web;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddJobSchedulingWeb(
        this ServiceConfigurationContext context,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            // TODO: temporary solution to get the scheduler hosted service to run (with its dependencies)
            // https://stackoverflow.com/questions/50394666/injecting-simple-injector-components-into-ihostedservice-with-asp-net-core-2-0#
            context.Services.AddSingleton<IHostedService>(sp =>
                    new JobSchedulerHostedService(sp.GetRequiredService<ILogger<JobSchedulerHostedService>>(), sp));
            return context;
        }
    }
}
