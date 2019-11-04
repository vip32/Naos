namespace Naos.Commands.Application.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using Humanizer;
    using Naos.Foundation;
    using NJsonSchema;
    using NSwag;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

    public class CommandRequestDocumentProcessor : IDocumentProcessor
    {
        private static readonly IDictionary<string, JsonSchema> Schemas = new Dictionary<string, JsonSchema>();
        private readonly IEnumerable<CommandRequestRegistration> registrations;

        public CommandRequestDocumentProcessor(IEnumerable<CommandRequestRegistration> registrations)
        {
            this.registrations = registrations;
        }

        public void Process(DocumentProcessorContext context)
        {
            foreach (var registrations in this.registrations.Safe()
                .Where(r => !r.Route.IsNullOrEmpty()).GroupBy(r => r.Route))
            {
                AddPathItem(context.Document.Paths, registrations.DistinctBy(r => r.RequestMethod), context);
            }
        }

        private static void AddPathItem(IDictionary<string, OpenApiPathItem> items, IEnumerable<CommandRequestRegistration> registrations, DocumentProcessorContext context)
        {
            var item = new OpenApiPathItem();

            foreach (var registration in registrations)
            {
                var method = registration.RequestMethod.ToLower();
                var operation = new OpenApiOperation
                {
                    Description = registration.OpenApiDescription ?? (registration.CommandType ?? typeof(object)).Name,
                    Summary = registration.OpenApiSummary + (registration.IsQueued ? " (queued)" : string.Empty),
                    OperationId = HashAlgorithm.ComputeHash($"{method} {registration.Route}"),
                    Tags = new[] { !registration.OpenApiGroupName.IsNullOrEmpty() ? $"{registration.OpenApiGroupPrefix} ({registration.OpenApiGroupName})" : registration.OpenApiGroupPrefix }.ToList(),
                    Produces = registration.OpenApiProduces.Safe(ContentType.JSON.ToValue()).Split(';').Distinct().ToList(),
                    //RequestBody = new OpenApiRequestBody{}
                };

                item.Add(method, operation);

                var hasResponseModel = registration.ResponseType?.Name.SafeEquals("object") == false && !registration.IsQueued;
                var description = registration.OpenApiResponseDescription ?? (hasResponseModel ? registration.ResponseType : null)?.FullPrettyName();
                var jsonSchema = context.SchemaGenerator.Generate(registration.ResponseType, context.SchemaResolver);
                // reuse some previously generated schemas, so schema $refs are avoided
                if (!description.IsNullOrEmpty())
                {
                    if (Schemas.ContainsKey(description))
                    {
                        jsonSchema = Schemas[description];
                    }
                    else
                    {
                        Schemas.Add(description, jsonSchema);
                    }
                }

                operation.Responses.Add(((int)registration.OnSuccessStatusCode).ToString(), new OpenApiResponse
                {
                    Description = description,
                    Schema = hasResponseModel ? jsonSchema : null,
                    //Examples = hasResponseModel ? Factory.Create(registration.ResponseType) : null // header?
                });
                operation.Responses.Add(((int)HttpStatusCode.BadRequest).ToString(), new OpenApiResponse
                {
                    Description = string.Empty,
                });
                operation.Responses.Add(((int)HttpStatusCode.InternalServerError).ToString(), new OpenApiResponse
                {
                    Description = string.Empty
                });

                AddResponseHeaders(operation, registration);
                AddOperationParameters(operation, method, registration, context);
            }

            if (item.Any() && !items.ContainsKey(registrations.First().Route))
            {
                items?.Add(registrations.First().Route, item);
            }
        }

        private static void AddResponseHeaders(OpenApiOperation operation, CommandRequestRegistration registration)
        {
            foreach (var response in operation.Responses)
            {
                response.Value.Headers.Add(CommandRequestHeaders.CommandId, new JsonSchema { Type = JsonObjectType.String });
                if (registration.IsQueued)
                {
                    response.Value.Headers.Add("Location", new JsonSchema { Type = JsonObjectType.String });
                }
            }
        }

        private static void AddOperationParameters(OpenApiOperation operation, string method, CommandRequestRegistration registration, DocumentProcessorContext context)
        {
            if (registration.CommandType != null)
            {
                if (method.SafeEquals("get") || method.SafeEquals("delete"))
                {
                    AddQueryOperation(operation, registration);
                }
                else if (method.SafeEquals("post") || method.SafeEquals("put") || method.SafeEquals(string.Empty))
                {
                    AddBodyOperation(operation, registration, context);
                }
                else
                {
                    // TODO: ignore for now, or throw? +log
                }
            }
        }

        private static void AddQueryOperation(OpenApiOperation operation, CommandRequestRegistration registration)
        {
            foreach (var property in registration.CommandType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // translate commandType properties to many OpenApiParameters
                if (!property.CanWrite || !property.CanRead)
                {
                    continue;
                }

                var type = JsonObjectType.String;
                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(short))
                {
                    type = JsonObjectType.Integer;
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(float) || property.PropertyType == typeof(long))
                {
                    type = JsonObjectType.Number;
                }
                else if (property.PropertyType == typeof(bool))
                {
                    type = JsonObjectType.Boolean;
                }
                else if (property.PropertyType == typeof(object))
                {
                    type = JsonObjectType.Object; // TODO: does not work for child objects
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    //Description = "request model",
                    Kind = registration.Route.Contains($"{{{property.Name}", System.StringComparison.OrdinalIgnoreCase)
                        ? OpenApiParameterKind.Path
                        : OpenApiParameterKind.Query,
                    Name = property.Name.Camelize(),
                    Type = type,
                });
            }
        }

        private static void AddBodyOperation(OpenApiOperation operation, CommandRequestRegistration registration, DocumentProcessorContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                //Description = "request model",
                Kind = OpenApiParameterKind.Body,
                Name = (registration.CommandType ?? typeof(object)).PrettyName(), //"model",
                Type = JsonObjectType.Object,
                Schema = CreateSchema(registration, context),
                //Example = registration.CommandType != null ? Factory.Create(registration.CommandType) : null //new Commands.Domain.EchoCommand() { Message = "test"},
            });
        }

        private static JsonSchema CreateSchema(CommandRequestRegistration registration, DocumentProcessorContext context)
        {
            return context.SchemaGenerator.Generate(registration.CommandType, context.SchemaResolver);

            //var schema = result.AllOf.FirstOrDefault();
            //if (schema != null)
            //{
            //    // workaround: remove invalid first $ref in allof https://github.com/RicoSuter/NSwag/issues/2119
            //    result.AllOf.Remove(schema);
            //}

            // remove some more $refs
            //foreach(var definition in result.Definitions.Safe())
            //{
            //    var s = definition.Value.AllOf.FirstOrDefault();
            //    if(s != null)
            //    {
            //        definition.Value.AllOf.Remove(s);
            //    }
            //}
        }
    }
}
