namespace Naos.Tracing.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISpanExporter
    {
        Task ExportAsync(
            IEnumerable<ISpan> spans,
            CancellationToken cancellationToken);
    }
}