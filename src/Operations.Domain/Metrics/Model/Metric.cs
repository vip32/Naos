namespace Naos.Operations.Domain
{
    using System;

    public class Metric
    {
        public Metric()
        {
            this.Timestamp = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset Timestamp { get; }

        public string Category { get; set; } // namespace (=servicedescriptor.name) (1)

        public string Name { get; set; } // metric (2)

        public string Type { get; set; } // dimkey (optional)

        public string Instance { get; set; } // dimval (optional)

        public int? Counter { get; protected set; }

        public double? Gauge { get; protected set; }

        public long? Timer { get; protected set; }

        public MetricTimerUnit? TimerUnit { get; protected set; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class MetricsPublisher // : injected where needed
    {
        public MetricsPublisher(/*IMediator mediator, ServiceDescripor serviceDescriptor*/)
        {
            //this.mediator = mediator;
        }

        public void Publish(Metric metric)
        {
            //metric.Category = serviceProvider.Name;
            //this.mediator.Publish(metric);
        }
    }

    public class AzureMonitoringMetricHandler // : mediator request handler<Metric>
    {
        public AzureMonitoringMetricHandler(string azureReourceId)
        {
            // https://blog.kloud.com.au/2019/01/16/automating-azure-instrumentation-and-monitoring-part-3-custom-metrics/
            // https://docs.microsoft.com/en-us/azure/azure-monitor/platform/metrics-custom-overview

            // get access token
            // POST https://{RegionId}.monitoring.azure.com{azureReourceId}/metrics";
        }
    }

    public class CounterMetric : Metric
    {
        public CounterMetric(string name, int value = 1)
        {
            this.Name = name;
            this.Counter = value;
        }
    }

    public class GaugeMetric : Metric
    {
        public GaugeMetric(string name, double value)
        {
            this.Name = name;
            this.Gauge = value;
        }
    }

    public class TimerMetric : Metric
    {
        public TimerMetric(string name, long value, MetricTimerUnit unit = MetricTimerUnit.Milliseconds)
        {
            this.Name = name;
            this.Timer = value;
            this.TimerUnit = unit;
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}
