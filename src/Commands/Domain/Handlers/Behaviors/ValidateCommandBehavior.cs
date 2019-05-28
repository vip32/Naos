namespace Naos.Core.Commands.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using FluentValidation;
    using Naos.Core.Common;

    public class ValidateCommandBehavior : ICommandBehavior
    {
        private readonly bool throwOnNotIsValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateCommandBehavior"/> class.
        /// </summary>
        /// <param name="throwOnNotIsValid">if set to <c>true</c> [throw on not is valid].</param>
        public ValidateCommandBehavior(bool throwOnNotIsValid = true)
        {
            this.throwOnNotIsValid = throwOnNotIsValid;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
        {
            EnsureArg.IsNotNull(request);

            var result = request.Validate();
            if(!result.IsValid)
            {
                // instead of cancel, throw an exception
                // TODO: log validation errors
                if(this.throwOnNotIsValid)
                {
                    throw new ValidationException($"{request.GetType().Name} has validation errors: " + result.Errors.Safe().Select(e => $"{e.PropertyName}={e}").ToString(", "), result.Errors);
                }
                else
                {
                    return await Task.FromResult(
                        new CommandBehaviorResult(
                            $"{request.GetType().Name} has validation errors: " + result.Errors.Safe().Select(e => $"{e.PropertyName}={e}").ToString(", "))).AnyContext();
                }
            }

            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
