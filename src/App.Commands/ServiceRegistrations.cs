namespace Naos.Core.App.Commands
{
    using System.Collections.Generic;
    using System.Reflection;
    using Naos.Core.Common;
    using SimpleInjector;

    public static class ServiceRegistrations
    {
        public static Container AddNaosAppCommands(
            this Container container,

            IEnumerable<Assembly> assemblies = null)
        {
            var allAssemblies = new List<Assembly> { typeof(ICommandBehavior).GetTypeInfo().Assembly };
            if (!assemblies.IsNullOrEmpty())
            {
                allAssemblies.AddRange(assemblies);
            }

            // TODO: improve this so command behavior types can be configured, as the sequence matters. for now ALL behaviors are simply added
            container.Collection.Register<ICommandBehavior>(allAssemblies.DistinctBy(a => a.FullName)); // register all command behaviors

            return container;
        }
    }
}
