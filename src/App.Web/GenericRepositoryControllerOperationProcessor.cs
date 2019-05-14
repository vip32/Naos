namespace Naos.Core.App.Web
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using NSwag.SwaggerGeneration.Processors;
    using NSwag.SwaggerGeneration.Processors.Contexts;

    /// <summary>
    /// Corrects (groups by entity) the tags used for the generic repositories.
    /// </summary>
    /// <seealso cref="NSwag.SwaggerGeneration.Processors.IOperationProcessor" />
    public class GenericRepositoryControllerOperationProcessor : IOperationProcessor
    {
        /// <summary>
        /// Processes the specified method information.
        /// </summary>
        /// <param name="context">The processor context.</param>
        /// <returns>
        /// true if the operation should be added to the Swagger specification.
        /// </returns>
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
