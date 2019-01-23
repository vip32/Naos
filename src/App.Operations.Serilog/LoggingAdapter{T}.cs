namespace Naos.Core.Commands.Operations.Serilog
{
    using System;
    using Microsoft.Extensions.Logging;

    public class LoggingAdapter<T> : ILogger<T>
    {
        private readonly ILogger adaptee;

        public LoggingAdapter(ILoggerFactory factory)
        {
            this.adaptee = factory.CreateLogger<T>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this.adaptee.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.adaptee.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.adaptee.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}