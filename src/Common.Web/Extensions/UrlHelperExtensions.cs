namespace Naos.Core.Common.Web
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// <see cref="IUrlHelper"/> extension methods.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name and
        /// route values.
        /// </summary>
        /// <param name="source">The URL helper.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteAction(
            this IUrlHelper source,
            string actionName,
            string controllerName,
            object routeValues = null)
        {
            EnsureArg.IsNotNull(source, nameof(source));

            return source.Action(actionName, controllerName, routeValues, source.ActionContext.HttpContext.Request.Scheme);
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified path by using the specified path. Converts a
        /// virtual (relative) path to an application absolute path.
        /// </summary>
        /// <param name="source">The URL helper.</param>
        /// <param name="path">The content path.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsolutePath(
            this IUrlHelper source,
            string path)
        {
            EnsureArg.IsNotNull(source, nameof(source));

            var request = source.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), source.Content(path)).ToString();
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified route by using the route name and route values.
        /// </summary>
        /// <param name="source">The URL helper.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteRoute(
            this IUrlHelper source,
            string routeName,
            object routeValues = null)
        {
            EnsureArg.IsNotNull(source, nameof(source));

            return source.RouteUrl(routeName, routeValues, source.ActionContext.HttpContext.Request.Scheme);
        }
    }
}