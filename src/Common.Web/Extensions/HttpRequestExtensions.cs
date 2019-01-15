namespace Naos.Core.Common.Web
{
    using System;
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static Uri Uri(this HttpRequest source)
        {
            if (source == null)
            {
                return null;
            }

            return new Uri($"{source.Scheme}://{source.Host}{source.Path}{source.QueryString}");
        }
    }
}
