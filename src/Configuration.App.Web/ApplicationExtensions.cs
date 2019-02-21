namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;

    /// <summary>
    /// Extension methods for the naos middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        public static IApplicationBuilder UseNaos(
            this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app;
        }
    }
}
