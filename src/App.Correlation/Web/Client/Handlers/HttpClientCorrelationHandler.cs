namespace Naos.Core.Correlation.Web
{
    using System.Net.Http;

    public class HttpClientCorrelationHandler : DelegatingHandler
    {
        // TODO: add the current correlationid header to the outgoing CLIENT request (get from ICorrelationContextAccessor)
    }
}
