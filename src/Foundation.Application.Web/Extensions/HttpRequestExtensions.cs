namespace Naos.Foundation.Application
{
    using System;
    using System.Net;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static Uri GetUri(this HttpRequest source, bool hostOnly = false)
        {
            if (source == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.Append(source.Scheme).Append("://");

            if (source.Host.HasValue)
            {
                builder.Append(source.Host.Value);
            }
            else
            {
                // HTTP 1.0 request with NO host header would result in empty Host. Use placeholder to avoid incorrect URL like "http:///"
                builder.Append("UNKNOWN-HOST");
            }

            if (source.PathBase.HasValue && !hostOnly)
            {
                builder.Append(source.PathBase.Value);
            }

            if (source.Path.HasValue && !hostOnly)
            {
                builder.Append(source.Path.Value);
            }

            if (source.QueryString.HasValue && !hostOnly)
            {
                builder.Append(source.QueryString);
            }

            return new Uri(builder.ToString());
        }

        public static bool IsLocal(this HttpRequest source)
        {
            // https://stackoverflow.com/a/41242493/7860424
            var connection = source.HttpContext.Connection;
            if (IsIpAddressSet(connection.RemoteIpAddress))
            {
                return IsIpAddressSet(connection.LocalIpAddress)
                    //if local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    //else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;

            bool IsIpAddressSet(IPAddress address)
            {
                return address != null && address.ToString() != "::1";
            }
        }
    }
}
