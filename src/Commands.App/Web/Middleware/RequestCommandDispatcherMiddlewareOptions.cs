namespace Naos.Core.Commands.App.Web
{
    using System;

    /// <summary>
    /// Options for command.
    /// </summary>
    public class RequestCommandDispatcherMiddlewareOptions
    {
        public string Route { get; set; }

        public Type CommandType { get; set; }

        public string RequestMethod { get; set; }
    }
}
