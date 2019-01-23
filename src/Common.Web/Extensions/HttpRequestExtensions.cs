namespace Naos.Core.Common.Web
{
    using System;
    using System.Net;
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
