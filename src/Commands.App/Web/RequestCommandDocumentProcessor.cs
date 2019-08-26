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
            foreach (var registration in this.registrations.Safe()
                .Where(r => !r.Route.IsNullOrEmpty()))
            {
                AddPathItem(context.Document.Paths, registration, context);
            }
        }

        private static void AddPathItem(IDictionary<string, OpenApiPathItem> paths, RequestCommandRegistration registration, DocumentProcessorContext context)
        {
            var item = new OpenApiPathItem();

            foreach (var method in registration.RequestMethod.Safe("post").Split(';').Distinct())
            {
                //{
                //    [method.ToLower()] = new OpenApiOperation
                //    {
                //        Description = registration.OpenApiDescription ?? (registration.CommandType ?? typeof(object)).Name,
                //        Summary = registration.OpenApiSummary,
                //        OperationId = HashAlgorithm.ComputeHash($"{method} {registration.Route}"),
                //        Tags = new[] { "Naos Commands" }.ToList(),
                //        Produces = registration.OpenApiProduces.Safe(ContentType.JSON.ToValue()).Split(';').Distinct().ToList(),
                //        //RequestBody = new OpenApiRequestBody{}
                //    }
                //};

                var operation = new OpenApiOperation
                {
                    Description = registration.OpenApiDescription ?? (registration.CommandType ?? typeof(object)).Name,
                    Summary = registration.OpenApiSummary,
                    OperationId = HashAlgorithm.ComputeHash($"{method} {registration.Route}"),
                    Tags = new[] { "Naos Commands" }.ToList(),
                    Produces = registration.OpenApiProduces.Safe(ContentType.JSON.ToValue()).Split(';').Distinct().ToList(),
                    //RequestBody = new OpenApiRequestBody{}
                };

                item.Add(method.ToLower(), operation);

                var hasResponseModel = registration.ResponseType?.Name.SafeEquals("object") == false;
                operation.Responses.Add(registration.ResponseStatusCodeOnSuccess.ToString(), new OpenApiResponse
                {
                    Description = registration.OpenApiResponseDescription ?? (hasResponseModel ? registration.ResponseType : null)?.Name,
                    Schema = hasResponseModel ? context.SchemaGenerator.Generate(registration.ResponseType) : null,
                    //Examples = hasResponseModel ? Factory.Create(registration.ResponseType) : null // header?
                });

                AddOperationParameters(operation, method, registration, context);
            }

            if (item.Any())
            {
                paths?.Add(registration.Route, item);
            }
        }

        private static void AddOperationParameters(OpenApiOperation operation, string method, RequestCommandRegistration registration, DocumentProcessorContext context)
        {
            if (registration.CommandType != null)
            {
                if (method.SafeEquals("get") || method.SafeEquals("delete"))
                {
                    // request querystring
                    AddQueryOperation(operation, registration);
                }
                else if (method.SafeEquals("post") || method.SafeEquals("put") || method.SafeEquals(string.Empty))
                {
                    // request body
                    AddBodyOperation(operation, registration, context);
                }
                else
                {
                    // TODO: ignore for now, or throw? +log
                }
            }
        }

        private static void AddQueryOperation(OpenApiOperation operation, RequestCommandRegistration registration)
        {
            foreach (var property in registration.CommandType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // translate commandType properties to many OpenApiParameters (OpenApiParameterKind.FormData/Query) >> Reflection!
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
                    type = JsonObjectType.Object;
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    //Description = "request model",
                    Kind = OpenApiParameterKind.Query,
                    Name = property.Name.Camelize(),
                    Type = type, // TODO: depend on prop type!
                                 //Schema = schema,
                                 //Example = registration.CommandType != null ? Factory.Create(registration.CommandType) : null //new Commands.Domain.EchoCommand() { Message = "test"},
                });
            }
        }

        private static void AddBodyOperation(OpenApiOperation operation, RequestCommandRegistration registration, DocumentProcessorContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                //Description = "request model",
                Kind = OpenApiParameterKind.Body,
                Name = (registration.CommandType ?? typeof(object)).Name, //"model",
                Type = JsonObjectType.Object,
                Schema = CreateSchema(registration, context),
                //Example = registration.CommandType != null ? Factory.Create(registration.CommandType) : null //new Commands.Domain.EchoCommand() { Message = "test"},
            });
        }

        private static JsonSchema CreateSchema(RequestCommandRegistration registration, DocumentProcessorContext context)
        {
            var result = context.SchemaGenerator.Generate(registration.CommandType);
            var schema = result.AllOf.FirstOrDefault();
            if (schema != null)
            {
                // workaround: remove invalid first $ref in allof https://github.com/RicoSuter/NSwag/issues/2119
                result.AllOf.Remove(schema);
            }

            return result;
        }
    }
}
