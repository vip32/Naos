namespace Naos.Core.App.Web
{
    using System.Linq;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

    public class RequestCommandDocumentProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            // TODO: foreach RequestCommandRegistration add the following swagger path
            var item = new NSwag.OpenApiPathItem
            {
                {
                    "get",
                    new NSwag.OpenApiOperation
                    {
                        OperationId = "command1",
                        Summary = "test operation",
                        Description = "test operation long",
                        Tags = new[] { "Naos Commands" }.ToList(),
                        Produces = new[] { "application/json" }.ToList(),
                    }
                }
            };

            context.Document.Paths.Add("/api/test", item);
        }
    }
}
