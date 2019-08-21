namespace Naos.Core.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using NJsonSchema;
    using NJsonSchema.Generation;
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
            //var settings = new JsonSchemaGeneratorSettings();
            //var schema = new JsonSchema();
            //var schemaResolver = new JsonSchemaResolver(schema, settings); // used to add and retrieve schemas from the 'definitions'
            //var schemaGenerator = new JsonSchemaGenerator(settings);

            foreach (var registration in this.registrations.Safe()
                .Where(r => !r.Route.IsNullOrEmpty()))
            {
                var item = new OpenApiPathItem
                {
                    {
                        registration.RequestMethod?.ToLower() ?? "post",
                        new OpenApiOperation
                        {
                            Description = "test operation long",
                            OperationId = Guid.NewGuid().ToString(),
                            Summary = "test operation",
                            Tags = new[] { "Naos Commands" }.ToList(),
                            Produces = new[] { "application/json" }.ToList(),
                            //RequestBody = new OpenApiRequestBody{}
                        }
                    }
                };

                if (registration.CommandType != null)
                {
                    var schema = context.SchemaGenerator.Generate(registration.CommandType);
                    // workaround: remove invalid first $ref in allof https://github.com/RicoSuter/NSwag/issues/2119
                    var firstSchema = schema.AllOf.FirstOrDefault();
                    if (firstSchema != null)
                    {
                        schema.AllOf.Remove(firstSchema);
                    }

                    if (!registration.RequestMethod.SafeEquals("get"))
                    {
                        // put/post/...
                        item.Values.FirstOrDefault()?.Parameters.Add(new OpenApiParameter
                        {
                            //Description = "request model",
                            Kind = OpenApiParameterKind.Body,
                            Name = "model",
                            Type = JsonObjectType.Object,
                            Schema = schema,
                            //Example = registration.CommandType != null ? Factory.Create(registration.CommandType) : null //new Commands.Domain.EchoCommand() { Message = "test"},
                        });
                    }
                    else
                    {
                        // get / delete?
                        foreach (var property in registration.CommandType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            // translate commandType properties to many OpenApiParameters (OpenApiParameterKind.FormData/Query) >> Reflection!
                            if (property.PropertyType != typeof(string)
                                && property.PropertyType != typeof(int)
                                && property.PropertyType != typeof(decimal)
                                && property.PropertyType != typeof(float)
                                && property.PropertyType != typeof(bool)
                                && property.PropertyType != typeof(long))
                            {
                                continue;
                            }

                            if (!property.CanWrite || !property.CanRead)
                            {
                                continue;
                            }

                            item.Values.FirstOrDefault()?.Parameters.Add(new OpenApiParameter
                            {
                                //Description = "request model",
                                Kind = OpenApiParameterKind.Query,
                                Name = property.Name.Camelize(),
                                Type = JsonObjectType.String, // TODO: depend on prop type!
                                //Schema = schema,
                                //Example = registration.CommandType != null ? Factory.Create(registration.CommandType) : null //new Commands.Domain.EchoCommand() { Message = "test"},
                            });
                        }
                    }
                }

                item.Values.FirstOrDefault()?.Responses.Add("200", new OpenApiResponse
                {
                    //Description = "response model",
                    Schema = registration.ResponseType != typeof(object) ? context.SchemaGenerator.Generate(registration.ResponseType) : null,
                    //Examples = registration.ResponseType != null ? Factory.Create(registration.ResponseType) : null // header?
                });

                context.Document.Paths.Add(registration.Route, item);
            }
        }
    }
}
