namespace Naos.Core.App.Web
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using NSwag.SwaggerGeneration.Processors;
    using NSwag.SwaggerGeneration.Processors.Contexts;

    public class GeneratedRepositoryControllerOperationProcessor : IOperationProcessor
    {
        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            // update tags where equal 'Naos Entity Repository' to 'Naos Entity Repository (Entity type name)'
            foreach(var description in context.Document.Operations.Safe().ToList())
            {
                if(description.Operation != null)
                {
                    var index = description.Operation.Tags?.IndexOf("Naos Entity Repository");
                    if(index.HasValue && index.Value >= 0)
                    {
                        description.Operation.Tags.RemoveAt(index.Value);
                        description.Operation.Tags.Add($"Naos Entity Repository ({description.Operation.OperationId.SubstringTill("_")})");
                        var operationDescription = context.OperationDescription;
                        operationDescription.Path = operationDescription.Path.ToLower();
                    }

                    index = description.Operation.Tags?.IndexOf("Naos Echo");
                    if(index.HasValue && index.Value >= 0)
                    {
                        var operationDescription = context.OperationDescription;
                        operationDescription.Path = operationDescription.Path.ToLower();
                    }
                }
            }

            return await Task.FromResult(true).AnyContext();
        }
    }
}
