namespace Naos.Core.App.Web
{
    using System.Linq;
    using NSwag;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

    public class RequestCommandDocumentProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            // TODO: foreach RequestCommandRegistration add the following swagger path
            var item = new OpenApiPathItem
            {
                {
                    "post",
                    new OpenApiOperation
                    {
                        OperationId = "command1",
                        Summary = "test operation",
                        Description = "test operation long",
                        Tags = new[] { "Naos Commands" }.ToList(),
                        Produces = new[] { "application/json" }.ToList(),
                        //RequestBody = new OpenApiRequestBody{}
                    }
                }
            };

            item.Values.FirstOrDefault()?.Parameters.Add(new OpenApiParameter
            {
                Kind = OpenApiParameterKind.Body,
                Name = "model",
                Type = NJsonSchema.JsonObjectType.Object,
            });

            context.Document.Paths.Add("/api/test", item);
        }
    }
}
