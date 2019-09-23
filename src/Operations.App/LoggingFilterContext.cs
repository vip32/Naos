namespace Naos.Operations.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Operations.Domain;
    using Naos.RequestFiltering.App;

    public static class LoggingFilterContext
    {
        public static void Prepare(FilterContext context)
        {
            // environment (default: current environment)
            if (!context.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Environment))))
            {
                context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Environment), CriteriaOperator.Equal, Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production"));
            }

            // message
            if (!context.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Message))))
            {
                context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Message), CriteriaOperator.NotEqual, string.Empty));
            }

            // level (default: >= Debug)
            if (!context.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Level))))
            {
                context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                //context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
            }
            else
            {
                var criteria = context.Criterias.FirstOrDefault(c => c.Name.SafeEquals(nameof(LogEvent.Level)));
                context.Criterias = context.Criterias.Where(c => c != criteria); // filter

                if (criteria.Value != null)
                {
                    if (criteria.Value.ToString().SafeEquals(nameof(LogLevel.Debug)))
                    {
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                    }
                    else if (criteria.Value.ToString().SafeEquals(nameof(LogLevel.Information)))
                    {
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                    }
                    else if (criteria.Value.ToString().SafeEquals(nameof(LogLevel.Warning)))
                    {
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Information)));
                    }
                    else if (criteria.Value.ToString().SafeEquals(nameof(LogLevel.Error))
                        || criteria.Value.ToString().SafeEquals(nameof(LogLevel.Critical)))
                    {
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Information)));
                        context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Warning)));
                    }
                }
            }

            // time range (default: last 7 days)
            if (!context.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Ticks))))
            {
                if (!context.Criterias.SafeAny(c => c.Name.SafeEquals("Epoch")))
                {
                    // add default range based on ticks
                    context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.LessThanOrEqual, DateTime.UtcNow.Ticks));
                    context.Criterias = context.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.GreaterThanOrEqual, DateTime.UtcNow.AddHours(-24 * 7).Ticks));
                }
                else
                {
                    var criterias = new List<Criteria>();
                    // convert provided epoch criterias to tick criterias
                    foreach (var criteria in context.Criterias.Where(c => c.Name.SafeEquals("Epoch")))
                    {
                        criterias.Add(new Criteria(nameof(LogEvent.Ticks), criteria.Operator, Extensions.FromEpoch(criteria.Value.To<long>()).Ticks));
                    }

                    context.Criterias = context.Criterias.Where(c => !c.Name.SafeEquals("Epoch")); // filter epoch
                    context.Criterias = context.Criterias.Insert(criterias);
                }
            }

            //foreach(var criteria in context.Criterias)
            //{
            //    await this.HttpContext.Response.WriteAsync($"criteria: {criteria}<br/>");
            //}

            context.Take ??= 1000; // get amount per request, repeat while logevents.ticks >= past

            //await foreach(var name in this.service.GetLogEventsAsync(context))
            //{
            //    this.logger.LogInformation(name);
            //}
        }
    }
}
