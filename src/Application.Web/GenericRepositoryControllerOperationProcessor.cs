namespace Naos.Application.Web
{
    using System.Linq;
    using Naos.Foundation;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;

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
        public bool Process(OperationProcessorContext context)
        {
            // update tags where equal 'Naos Entity Repository' to 'Naos Entity Repository (Entity type name)'
            foreach (var description in context.Document.Operations.Safe().ToList())
            {
                if (description.Operation != null)
                {
                    var index = description.Operation.Tags?.IndexOf("Naos Entity Repository");
                    if (index.HasValue && index.Value >= 0)
                    {
                        description.Operation.Tags.RemoveAt(index.Value);
                        description.Operation.Tags.Add($"Naos Entity Repository ({description.Operation.OperationId.SliceTill("_")})");
                        var operationDescription = context.OperationDescription;
                        operationDescription.Path = operationDescription.Path.ToLower();
                    }

                    index = description.Operation.Tags?.IndexOf("Naos Echo");
                    if (index.HasValue && index.Value >= 0)
                    {
                        var operationDescription = context.OperationDescription;
                        operationDescription.Path = operationDescription.Path.ToLower();
                    }
                }
            }

            return true;
        }
    }
}
