namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;

    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Executes the specified action if the specified <paramref name="condition"/> is <c>true</c> which can be
        /// used to conditionally configure the MVC services.
        /// </summary>
        /// <param name="source">The services collection.</param>
        /// <param name="condition">If set to <c>true</c> the action is executed.</param>
        /// <param name="action">The action used to configure the MVC services.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection AddIf(
            this IServiceCollection source,
            bool condition,
            Func<IServiceCollection, IServiceCollection> action)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(action, nameof(action));

            if (condition)
            {
                source = action(source);
            }

            return source;
        }

        /// <summary>
        /// Executes the specified <paramref name="ifAction"/> if the specified <paramref name="condition"/> is
        /// <c>true</c>, otherwise executes the <paramref name="elseAction"/>. This can be used to conditionally
        /// configure the MVC services.
        /// </summary>
        /// <param name="source">The services collection.</param>
        /// <param name="condition">If set to <c>true</c> the <paramref name="ifAction"/> is executed, otherwise the
        /// <paramref name="elseAction"/> is executed.</param>
        /// <param name="ifAction">The action used to configure the MVC services if the condition is <c>true</c>.</param>
        /// <param name="elseAction">The action used to configure the MVC services if the condition is <c>false</c>.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection AddIfElse(
            this IServiceCollection source,
            bool condition,
            Func<IServiceCollection, IServiceCollection> ifAction,
            Func<IServiceCollection, IServiceCollection> elseAction)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(ifAction, nameof(ifAction));
            EnsureArg.IsNotNull(elseAction, nameof(elseAction));

            if (condition)
            {
                source = ifAction(source);
            }
            else
            {
                source = elseAction(source);
            }

            return source;
        }
    }
}