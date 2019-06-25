namespace Naos.Foundation.Application.Extensions
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// <see cref="IWebHostBuilder"/> extension methods.
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Executes the specified action if the specified <paramref name="condition"/> is <c>true</c> which can be
        /// used to conditionally add to the host builder.
        /// </summary>
        /// <param name="source">The host builder.</param>
        /// <param name="condition">If set to <c>true</c> the action is executed.</param>
        /// <param name="action">The action used to add to the host builder.</param>
        /// <returns>The same host builder.</returns>
        public static IWebHostBuilder UseIf(
            this IWebHostBuilder source,
            bool condition,
            Func<IWebHostBuilder, IWebHostBuilder> action)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(action, nameof(action));

            if(condition)
            {
                source = action(source);
            }

            return source;
        }

        /// <summary>
        /// Executes the specified action if the specified <paramref name="condition"/> is <c>true</c> which can be
        /// used to conditionally add to the host builder.
        /// </summary>
        /// <param name="source">The host builder.</param>
        /// <param name="condition">If <c>true</c> is returned the action is executed.</param>
        /// <param name="action">The action used to add to the host builder.</param>
        /// <returns>The same host builder.</returns>
        public static IWebHostBuilder UseIf(
            this IWebHostBuilder source,
            Func<IWebHostBuilder, bool> condition,
            Func<IWebHostBuilder, IWebHostBuilder> action)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(condition, nameof(condition));
            EnsureArg.IsNotNull(action, nameof(action));

            if(condition(source))
            {
                source = action(source);
            }

            return source;
        }

        /// <summary>
        /// Executes the specified <paramref name="ifAction"/> if the specified <paramref name="condition"/> is
        /// <c>true</c>, otherwise executes the <paramref name="elseAction"/>. This can be used to conditionally add to
        /// the host builder.
        /// </summary>
        /// <param name="source">The host builder.</param>
        /// <param name="condition">If set to <c>true</c> the <paramref name="ifAction"/> is executed, otherwise the
        /// <paramref name="elseAction"/> is executed.</param>
        /// <param name="ifAction">The action used to add to the host builder if the condition is <c>true</c>.</param>
        /// <param name="elseAction">The action used to add to the host builder if the condition is <c>false</c>.</param>
        /// <returns>The same host builder.</returns>
        public static IWebHostBuilder UseIfElse(
            this IWebHostBuilder source,
            bool condition,
            Func<IWebHostBuilder, IWebHostBuilder> ifAction,
            Func<IWebHostBuilder, IWebHostBuilder> elseAction)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(ifAction, nameof(ifAction));
            EnsureArg.IsNotNull(elseAction, nameof(elseAction));

            if(condition)
            {
                source = ifAction(source);
            }
            else
            {
                source = elseAction(source);
            }

            return source;
        }

        /// <summary>
        /// Executes the specified <paramref name="ifAction"/> if the specified <paramref name="condition"/> is
        /// <c>true</c>, otherwise executes the <paramref name="elseAction"/>. This can be used to conditionally add to
        /// the host builder.
        /// </summary>
        /// <param name="source">The host builder.</param>
        /// <param name="condition">If <c>true</c> is returned the <paramref name="ifAction"/> is executed, otherwise the
        /// <paramref name="elseAction"/> is executed.</param>
        /// <param name="ifAction">The action used to add to the host builder if the condition is <c>true</c>.</param>
        /// <param name="elseAction">The action used to add to the host builder if the condition is <c>false</c>.</param>
        /// <returns>The same host builder.</returns>
        public static IWebHostBuilder UseIfElse(
            this IWebHostBuilder source,
            Func<IWebHostBuilder, bool> condition,
            Func<IWebHostBuilder, IWebHostBuilder> ifAction,
            Func<IWebHostBuilder, IWebHostBuilder> elseAction)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(condition, nameof(condition));
            EnsureArg.IsNotNull(ifAction, nameof(ifAction));
            EnsureArg.IsNotNull(elseAction, nameof(elseAction));

            if(condition(source))
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