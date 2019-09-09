namespace Naos.Core.Commands.App
{
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using FluentValidation;
    using Naos.Foundation;

    public class ValidateCommandBehavior : ICommandBehavior
    {
        private readonly bool throwOnNotIsValid;
        private ICommandBehavior next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateCommandBehavior"/> class.
        /// </summary>
        /// <param name="throwOnNotIsValid">if set to <c>true</c> [throw on not is valid].</param>
        public ValidateCommandBehavior(bool throwOnNotIsValid = true)
        {
            this.throwOnNotIsValid = throwOnNotIsValid;
        }

        public ICommandBehavior SetNext(ICommandBehavior next)
        {
            this.next = next;
            return next;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task ExecutePreHandleAsync<TResponse>(Command<TResponse> request, CommandBehaviorResult result)
        {
            EnsureArg.IsNotNull(request);

            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                // TODO: log validation errors
                if (this.throwOnNotIsValid)
                {
                    throw new ValidationException($"{request.GetType().Name} has validation errors: " + validationResult.Errors.Safe().Select(e => $"{e.PropertyName}={e}").ToString(", "), validationResult.Errors);
                }
                else
                {
                    result.SetCancelled($"{request.GetType().Name} has validation errors: " + validationResult.Errors.Safe().Select(e => $"{e.PropertyName}={e}").ToString(", "));
                }
            }

            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePreHandleAsync(request, result).AnyContext();
            }

            // terminate here
        }

        public async Task ExecutePostHandleAsync<TResponse>(CommandResponse<TResponse> response, CommandBehaviorResult result)
        {
            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePostHandleAsync(response, result).AnyContext();
            }
        }
    }
}
