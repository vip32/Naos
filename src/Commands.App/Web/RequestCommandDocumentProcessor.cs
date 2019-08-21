namespace Naos.Core.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using NSwag;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

    public class RequestCommandDocumentProcessor : IDocumentProcessor
    {
        private readonly IEnumerable<RequestCommandRegistration> registrations;

        public RequestCommandDocumentProcessor(IEnumerable<RequestCommandRegistration> registrations)
        {
            this.registrations = registrations;
        }

        public void Process(DocumentProcessorContext context)
        {
            foreach (var registration in this.registrations.Safe())
            {
                // TODO: foreach RequestCommandRegistration add the following swagger path
                var item = new OpenApiPathItem
                {
                    {
                        registration.RequestMethod?.ToLower(),
                        new OpenApiOperation
                        {
                            OperationId = Guid.NewGuid().ToString(),
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

                context.Document.Paths.Add(registration.Route, item);
            }
        }
    }
}
